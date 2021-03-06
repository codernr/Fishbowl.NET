using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Fishbowl.Net.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fishbowl.Net.Server.Services
{
    public class GameContext : IAsyncDisposable
    {
        public AsyncGame Game
        {
            get
            {
                this.timer.Restart();
                return this.game ?? throw new InvalidOperationException();
            }
        }

        public int WordCount => this.gameSetup.WordCount;

        public event Action<GameContext>? GameFinished;

        private readonly List<Player> players = new();

        private readonly GameSetupViewModel gameSetup;

        private readonly IGroupHubContext groupHubContext;

        private readonly Timer timer;

        private readonly GameOptions gameOptions;

        private readonly ILogger<GameContext> logger;

        private AsyncGame? game;

        private List<Team> Teams => this.teams ?? throw new InvalidOperationException();

        private List<Team>? teams;

        private bool isDisposing = false;

        public GameContext(
            GameSetupViewModel gameSetup,
            IGroupHubContext groupHubContext,
            Func<Func<Task>, Timer> timerFactory,
            IOptions<GameOptions> gameOptions,
            ILogger<GameContext> logger) =>
            (this.gameSetup, this.groupHubContext, this.timer, this.gameOptions, this.logger) =
            (gameSetup, groupHubContext, timerFactory(() => this.Abort("Common.Abort.Timeout")), gameOptions.Value, logger);

        public bool CanRegister(Guid playerId)
        {
            var result = this.groupHubContext.ContainsKey(playerId) || this.groupHubContext.Count < this.gameSetup.PlayerCount;
            this.Log(nameof(this.CanRegister), playerId, result);
            return result;
        }

        public async Task<bool> TryRegisterConnection(Guid playerId, string connectionId)
        {
            this.Log(nameof(this.TryRegisterConnection), playerId, connectionId);

            if (this.isDisposing)
            {
                this.logger.LogWarning("Disposing");
                return false;
            }

            this.timer.Restart();

            await this.groupHubContext.RegisterConnection(playerId, connectionId);
            await this.groupHubContext.Group().ReceivePlayerCount(
                new PlayerCountViewModel(this.gameSetup.PlayerCount, this.groupHubContext.Count, this.players.Count));

            var existingPlayer = this.players.SingleOrDefault(player => player.Id == playerId);

            var clientTask =
                existingPlayer is null  ? this.groupHubContext.Client(playerId).ReceiveSetupPlayer(this.gameSetup) :
                (this.teams is null     ? this.groupHubContext.Client(playerId).ReceiveWaitForOtherPlayers(existingPlayer.Map()) :
                (this.game is null      ? this.RestoreTeamSetup(existingPlayer, this.teams) :
                this.RestoreGame(existingPlayer, this.game)));

            await clientTask;

            this.logger.LogInformation("ConnectionRegistered");
            return true;
        }

        public Task RemoveConnection(string connectionId) => this.groupHubContext.RemoveConnection(connectionId);

        public async Task AddPlayer(AddPlayerViewModel player)
        {
            this.Log(nameof(this.AddPlayer), player);

            if (!this.groupHubContext.ContainsKey(player.Id))
            {
                throw new InvalidOperationException("Invalid player connection");
            }

            this.timer.Restart();

            var entity = player.Map();

            this.players.Add(entity);

            await this.groupHubContext.Group().ReceivePlayerCount(
                new PlayerCountViewModel(this.gameSetup.PlayerCount, this.groupHubContext.Count, this.players.Count));

            if (this.players.Count != this.gameSetup.PlayerCount)
            {
                await this.groupHubContext.Client(player.Id).ReceiveWaitForOtherPlayers(entity.Map());
                return;
            }

            await this.CreateTeams(this.players, this.gameSetup.TeamCount);
        }

        public async Task SetTeamName(TeamNameViewModel teamName)
        {
            this.Log(nameof(this.SetTeamName), teamName);

            this.Teams[teamName.Id].Name = teamName.Name;

            await this.groupHubContext
                .Client(this.Teams[teamName.Id].Players[0].Id)
                .ReceiveWaitForTeamSetup(new(this.Teams.Select(team => team.Map()).ToList()));

            await this.groupHubContext.Group().ReceiveTeamName(teamName);

            if (!this.Teams.Any(team => team.Name is null))
            {
                await this.StartGame(this.Teams, this.gameSetup.RoundTypes);
            }
        }

        private Task CreateTeams(List<Player> players, int teamCount)
        {
            this.teams = players.Randomize().ToList().CreateTeams(teamCount).ToList();

            var setupPlayerIds = this.teams
                .Select(team => team.Players.First())
                .Select(player => player.Id);

            TeamSetupViewModel data = this.Teams.Map();

            return Task.WhenAll(
                this.groupHubContext.Clients(setupPlayerIds).ReceiveSetTeamName(data),
                this.groupHubContext.GroupExcept(setupPlayerIds).ReceiveWaitForTeamSetup(data));
        }

        private async Task StartGame(List<Team> teams, IEnumerable<string> roundTypes)
        {
            this.Log(nameof(this.StartGame), teams, roundTypes);

            try
            {
                this.game = new AsyncGame(this.gameOptions, teams, roundTypes);
                this.SetEventHandlers();
                this.Game.Run();
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e is InvalidReturnValueException ? "InvalidReturnValue" : "InvalidPlayerCount");
                await this.Abort(e is InvalidReturnValueException ? "Common.Abort.InvalidReturnValue" : "Common.Abort.PlayerCount");
            }
        }

        private void SetEventHandlers()
        {
            this.Game.GameStarted += this.GameStarted;
            this.Game.GameFinished += this.OnGameFinished;
            this.Game.RoundStarted += this.RoundStarted;
            this.Game.RoundFinished += this.RoundFinished;
            this.Game.PeriodSetup += this.PeriodSetup;
            this.Game.PeriodStarted += this.PeriodStarted;
            this.Game.PeriodFinished += this.PeriodFinished;
            this.Game.ScoreAdded += this.ScoreAdded;
            this.Game.LastScoreRevoked += this.LastScoreRevoked;
            this.Game.WordSetup += this.WordSetup;
        }

        private async void GameStarted(Game game) =>
            await this.groupHubContext.Group().ReceiveGameStarted();

        private async void OnGameFinished(Game game)
        {
            await this.groupHubContext.Group().ReceiveGameFinished(game.Map());
            this.GameFinished?.Invoke(this);
        }

        private async void RoundStarted(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundStarted(round.Map());

        private async void RoundFinished(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundFinished(round.MapSummary());

        private async void PeriodSetup(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodSetup(
                period.Map(this.Game.Game.CurrentRound));

        private async void PeriodStarted(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodStarted(
                period.MapRunning(this.Game.Game.CurrentRound));

        private async void PeriodFinished(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodFinished(period.Map());

        private async void ScoreAdded(Score score) =>
            await this.groupHubContext.Group().ReceiveScoreAdded(score.Map());

        private async void LastScoreRevoked(Score score) =>
            await this.groupHubContext.Group().ReceiveLastScoreRevoked(score.Map());

        private async void WordSetup(Player player, Word word) =>
            await this.groupHubContext.Client(player.Id).ReceiveWordSetup(word.Map());

        private async Task Abort(string messageKey)
        {
            await this.groupHubContext.Group().ReceiveGameAborted(new GameAbortViewModel(messageKey));
            this.GameFinished?.Invoke(this);
        }

        private Task RestoreTeamSetup(Player player, List<Team> teams)
        {
            this.Log(nameof(this.RestoreTeamSetup), player, teams);

            var playerTeam = teams.First(team => team.Players.Any(teamPlayer => teamPlayer.Id == player.Id));

            var client = this.groupHubContext.Client(player.Id);

            return (playerTeam.Players[0].Id == player.Id && playerTeam.Name is null) ?
                client.ReceiveSetTeamName(this.Teams.Map()) : client.ReceiveWaitForTeamSetup(this.Teams.Map());
        }

        private async Task RestoreGame(Player player, AsyncGame game)
        {
            this.Log(nameof(this.RestoreGame), player, game);
            
            var round = game.Game.CurrentRound;
            var period = round.CurrentPeriod;
            var client = this.groupHubContext.Client(player.Id);

            await client.ReceiveRestoreState(player.Map());

            if (period.StartedAt is null)
            {
                await client.ReceivePeriodSetup(period.Map(round));
                return;
            }

            await client.ReceivePeriodStarted(period.MapRunning(round));

            if (player.Id == period.Player.Id)
            {
                await client.ReceiveWordSetup(game.Game.CurrentWord.Map());
            }
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.isDisposing) return;

            this.isDisposing = true;
            
            await this.timer.DisposeAsync();
            await this.groupHubContext.DisposeAsync();
        }

        private void Log(string methodName, object arg1) =>
            this.logger.LogInformation("{MethodName}: [{Arg1}]", methodName, arg1);

        private void Log(string methodName, object arg1, object arg2) =>
            this.logger.LogInformation("{MethodName}: [{Arg1}] [{Arg2}]", methodName, arg1, arg2);
    }
}