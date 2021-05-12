using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
using Fishbowl.Net.Shared.Exceptions;

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

        private readonly GameSetup gameSetup;

        private readonly IGroupHubContext groupHubContext;

        private readonly Timer timer;

        private AsyncGame? game;

        public GameContext(GameSetup gameSetup, IGroupHubContext groupHubContext, Func<Func<Task>, Timer> timerFactory) =>
            (this.gameSetup, this.groupHubContext, this.timer) =
            (gameSetup, groupHubContext, timerFactory(() => this.Abort("Common.Abort.Timeout")));

        public async Task RegisterConnection(Guid playerId, string connectionId)
        {
            this.timer.Restart();

            await this.groupHubContext.RegisterConnection(playerId, connectionId);
            await this.groupHubContext.Group().ReceivePlayerCount(
                new PlayerCountViewModel(this.players.Count, this.gameSetup.PlayerCount));

            var existingPlayer = this.players.SingleOrDefault(player => player.Id == playerId);

            if (existingPlayer is null)
            {
                await this.groupHubContext.Client(playerId).ReceiveSetupPlayer(this.gameSetup);
                return;
            }

            if (this.game is null)
            {
                await this.groupHubContext.Client(playerId).ReceiveWaitForOtherPlayers(existingPlayer);
                return;
            }

            await this.Restore(existingPlayer, this.game);
        }

        public async Task RemoveConnection(string connectionId)
        {
            await this.groupHubContext.RemoveConnection(connectionId);
        }

        public async Task AddPlayer(Player player)
        {
            if (!this.groupHubContext.ContainsKey(player.Id))
            {
                throw new InvalidOperationException("Invalid player connection");
            }

            this.timer.Restart();

            this.players.Add(player);

            await this.groupHubContext.Group().ReceivePlayerCount(
                new PlayerCountViewModel(this.players.Count, this.gameSetup.PlayerCount));

            if (this.players.Count != this.gameSetup.PlayerCount)
            {
                await this.groupHubContext.Client(player.Id).ReceiveWaitForOtherPlayers(player);
                return;
            }

            await this.StartGame(this.gameSetup.TeamCount, this.gameSetup.RoundTypes, this.players);
        }

        private async Task StartGame(int teamCount, IEnumerable<string> roundTypes, IEnumerable<Player> players)
        {
            try
            {
                this.game = new AsyncGame(teamCount, roundTypes, players);
                this.SetEventHandlers();
                this.Game.Run();
            }
            catch (ArgumentException e)
            {
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
            await this.groupHubContext.Group().ReceiveGameStarted(game.Map());

        private async void OnGameFinished(Game game)
        {
            await this.groupHubContext.Group().ReceiveGameFinished(game.MapSummary());
            this.GameFinished?.Invoke(this);
        }

        private async void RoundStarted(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundStarted(round.Map());

        private async void RoundFinished(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundFinished(round.MapSummary());

        private async void PeriodSetup(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodSetup(
                period.Map(this.Game.Game.CurrentRound()));

        private async void PeriodStarted(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodStarted(
                period.MapRunning(this.Game.Game.CurrentRound()));

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

        private async Task Restore(Player player, AsyncGame game)
        {
            var round = game.Game.CurrentRound();
            var period = round.CurrentPeriod();
            var client = this.groupHubContext.Client(player.Id);

            await client.RestoreGameState(player, round);

            if (period.StartedAt is null)
            {
                await client.ReceivePeriodSetup(period.Map(round));
                return;
            }

            await client.ReceivePeriodStarted(period.MapRunning(round));

            if (player == period.Player)
            {
                await client.ReceiveWordSetup(game.Game.CurrentWord().Map());
            }
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual ValueTask DisposeAsyncCore() => this.groupHubContext.DisposeAsync();
    }
}