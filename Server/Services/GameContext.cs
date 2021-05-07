using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Server.Services
{
    public class GameContext : IAsyncDisposable
    {
        public AsyncGame Game => this.game ?? throw new InvalidOperationException();

        public int WordCount => this.gameSetup.WordCount;

        public event Action<GameContext>? GameFinished;

        private readonly List<Player> players = new();

        private readonly GameSetup gameSetup;

        private readonly IGroupHubContext groupHubContext;

        private AsyncGame? game;

        public GameContext(GameSetup gameSetup, IGroupHubContext groupHubContext) =>
            (this.gameSetup, this.groupHubContext) = (gameSetup, groupHubContext);

        public async Task RegisterConnection(Guid playerId, string connectionId)
        {
            await this.groupHubContext.RegisterConnection(playerId, connectionId);
            await this.groupHubContext.Group().ReceiveConnectionCount(this.groupHubContext.ConnectionCount);

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
            await this.groupHubContext.Group().ReceiveConnectionCount(this.groupHubContext.ConnectionCount);
        }

        public void AddPlayer(Player player)
        {
            if (!this.groupHubContext.ContainsKey(player.Id))
            {
                throw new InvalidOperationException("Invalid player connection");
            }

            this.players.Add(player);

            if (this.players.Count != this.groupHubContext.ConnectionCount)
            {
                this.groupHubContext.Client(player.Id).ReceiveWaitForOtherPlayers(player);
                return;
            }

            this.StartGame(this.gameSetup.TeamCount, this.gameSetup.RoundTypes, this.players);
        }

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
            this.Game.LastScoreRevoked += this.LastScoreRevoked;
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

        private async void LastScoreRevoked(Score score) =>
            await this.groupHubContext.Group().ReceiveLastScoreRevoked(score);

        private async void WordSetup(Player player, Word word) =>
            await this.groupHubContext.Client(player.Id).ReceiveWordSetup(word);

        private async Task Restore(Player player, AsyncGame game)
        {
            var round = game.Game.CurrentRound();
            var period = round.CurrentPeriod();
            var client = this.groupHubContext.Client(player.Id);

            await client.RestoreGameState(player, round);

            if (period.StartedAt is null)
            {
                await client.ReceivePeriodSetup(period);
                return;
            }

            await client.ReceivePeriodStarted(period);

            if (player == period.Player)
            {
                await client.ReceiveWordSetup(game.Game.CurrentWord());
            }
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual ValueTask DisposeAsyncCore() => this.groupHubContext.DisposeAsync();
    }
}