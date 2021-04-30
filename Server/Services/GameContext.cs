using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Server.Services
{
    public class GameContext : IAsyncDisposable
    {
        public AsyncGame Game => this.game ?? throw new InvalidOperationException();

        public bool Running => this.game is not null;

        public int WordCount { get; private set; } = 2;

        public event Action<GameContext>? GameFinished;

        private readonly List<Player> players = new();

        private readonly IGroupHubContext groupHubContext;

        private AsyncGame? game;

        private GameSetup? setup;

        public GameContext(int wordCount, IGroupHubContext groupHubContext) =>
            (this.WordCount, this.groupHubContext) = (wordCount, groupHubContext);

        public void SetupGame(GameSetup request) => this.setup = request;

        public Task RegisterConnection(Guid playerId, string connectionId) =>
            this.groupHubContext.RegisterConnection(playerId, connectionId);

        public Task RemoveConnection(string connectionId) =>
            this.groupHubContext.RemoveConnection(connectionId);

        public void AddPlayer(Player player)
        {
            if (!this.groupHubContext.ContainsKey(player.Id))
            {
                throw new InvalidOperationException("Invalid player connection");
            }

            this.players.Add(player);

            if (this.players.Count == this.groupHubContext.ConnectionCount)
            {
                if (this.setup is null)
                {
                    this.groupHubContext.Group().ReceiveGameAborted("Game setup is missing.");
                    this.GameFinished?.Invoke(this);
                    return;
                }

                this.StartGame(this.setup.TeamCount, this.setup.RoundTypes, this.players);
            }
        }

        public Game GetGameData() => this.Game.Game;

        private void StartGame(int teamCount, IEnumerable<string> roundTypes, IEnumerable<Player> players)
        {
            try
            {
                this.game = new AsyncGame(teamCount, roundTypes, players);
                this.SetEventHandlers();
                this.Game.Run();
            }
            catch (ArgumentException e)
            {
                this.groupHubContext.Group().ReceiveGameAborted(e.Message);
                this.GameFinished?.Invoke(this);
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
            this.Game.WordSetup += this.WordSetup;
        }

        private async void GameStarted(Game game) =>
            await this.groupHubContext.Group().ReceiveGameStarted(game);

        private async void OnGameFinished(Game game)
        {
            await this.groupHubContext.Group().ReceiveGameFinished(game);
            this.GameFinished?.Invoke(this);
        }

        private async void RoundStarted(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundStarted(round);

        private async void RoundFinished(Round round) =>
            await this.groupHubContext.Group().ReceiveRoundFinished(round);

        private async void PeriodSetup(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodSetup(period);

        private async void PeriodStarted(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodStarted(period);

        private async void PeriodFinished(Period period) =>
            await this.groupHubContext.Group().ReceivePeriodFinished(period);

        private async void ScoreAdded(Score score) =>
            await this.groupHubContext.Group().ReceiveScoreAdded(score);

        private async void WordSetup(Player player, Word word) =>
            await this.groupHubContext.Client(player.Id).ReceiveWordSetup(word);

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual ValueTask DisposeAsyncCore() => this.groupHubContext.DisposeAsync();
    }
}