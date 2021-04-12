using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Data;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public class GameContext
    {
        public AsyncGame Game => this.game ?? throw new InvalidOperationException();

        private AsyncGame? game;

        public string Password { get; private set; }

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        private readonly Map<string, Player> players = new();

        private readonly List<string> connections = new();

        public GameContext(string password, IHubContext<GameHub, IGameClient> hubContext)
        {
            this.hubContext = hubContext;
            this.Password = password;
            this.SetEventHandlers();
        }

        public void RegisterConnection(string connectionId)
        {
            if (!this.connections.Contains(connectionId))
            {
                this.connections.Add(connectionId);
            }
        }

        public void CreateGame(int teamCount, IEnumerable<string> roundTypes)
        {
            if (this.game is not null)
            {
                throw new InvalidOperationException("The game is already created");
            }

            this.game = new AsyncGame(teamCount, roundTypes);

            this.Game.Run();
        }

        public void RemoveConnection(string connectionId)
        {
            if (this.players.ContainsKey(connectionId))
            {
                this.players.Remove(connectionId);
            }

            this.connections.Remove(connectionId);
        }

        public void AddPlayer(string connectionId, Player player)
        {
            this.players.Add(connectionId, player);

            this.Game.AddPlayer(player);

            if (this.players.Count == this.connections.Count)
            {
                this.Game.PlayersSet();
            }
        }

        private void SetEventHandlers()
        {
            this.Game.GameStarted += this.GameStarted;
            this.Game.GameFinished += this.GameFinished;
            this.Game.RoundStarted += this.RoundStarted;
            this.Game.RoundFinished += this.RoundFinished;
            this.Game.PeriodSetup += this.PeriodSetup;
            this.Game.PeriodStarted += this.PeriodStarted;
            this.Game.PeriodFinished += this.PeriodFinished;
            this.Game.ScoreAdded += this.ScoreAdded;
            this.Game.WordSetup += this.WordSetup;
        }

        private async void GameStarted(Game game) =>
            await this.hubContext.Clients.Group(this.Password).ReceiveGameStarted(game);

        private async void GameFinished(Game game) =>
            await this.hubContext.Clients.Group(this.Password).ReceiveGameFinished(game);

        private async void RoundStarted(Round round) =>
            await this.hubContext.Clients.Group(this.Password).ReceiveRoundStarted(round);

        private async void RoundFinished(Round round) =>
            await this.hubContext.Clients.Group(this.Password).ReceiveRoundFinished(round);

        private async void PeriodSetup(Period period) =>
            await this.hubContext.Clients.Group(this.Password).ReceivePeriodSetup(period);

        private async void PeriodStarted(Period period) =>
            await this.hubContext.Clients.Group(this.Password).ReceivePeriodStarted(period);

        private async void PeriodFinished(Period period) =>
            await this.hubContext.Clients.Group(this.Password).ReceivePeriodFinished(period);

        private async void ScoreAdded(Score score) =>
            await this.hubContext.Clients.Group(this.Password).ReceiveScoreAdded(score);

        private async void WordSetup(Player player, Word word) =>
            await this.hubContext.Clients.Clients(this.players[player]).ReceiveWordSetup(word);
    }
}