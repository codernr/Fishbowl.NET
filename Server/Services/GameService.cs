using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private readonly IHubContext<GameHub, IClient> hubContext;

        private readonly List<string> connections = new();

        private Dictionary<string, Player> players = new();

        private TaskCompletionSource<DateTimeOffset> inputAction = new();

        private int? teamCount;

        private IEnumerable<string>? roundTypes;

        private Game? game;

        private Task? gameLoop;

        private IEnumerable<string> RoundTypes => this.roundTypes ??
            throw new InvalidOperationException("Invalid game state: RoundTypes are not defined");

        private int TeamCount => this.teamCount ??
            throw new InvalidOperationException("Invalid game state: TeamCount is not defined");

        private Game Game => this.game ??
            throw new InvalidOperationException("Invalid game state: GameManager is not defined");

        public GameService(IHubContext<GameHub, IClient> hubContext) => this.hubContext = hubContext;

        public int RegisterConnection(string connectionId)
        {
            if (!this.connections.Contains(connectionId))
            {
                this.connections.Add(connectionId);
            }

            return this.connections.Count;
        }

        public void RemoveConnection(string connectionId)
        {
            if (this.players.ContainsKey(connectionId))
            {
                this.players.Remove(connectionId);
            }

            this.connections.Remove(connectionId);
        }

        public void SetTeamCount(int teamCount) => this.teamCount = teamCount;

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.roundTypes = roundTypes;

        public async Task SetPlayerAsync(string connectionId, Player player)
        {
            this.players.Add(connectionId, player);

            if (this.players.Count < this.connections.Count) return;
            
            await this.StartGame();
        }

        public Task StartPeriodAsync(DateTimeOffset timestamp)
        {
            this.Game.StartPeriod(timestamp);
            this.SetInput(timestamp);
            return this.hubContext.Clients.All.ReceivePeriodStart(timestamp);
        }

        public void FinishPeriod(DateTimeOffset timestamp) => this.Game.FinishPeriod(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.SetInput(timestamp);

        public Task AddScoreAsync(Score score)
        {
            this.Game.AddScore(score);
            return this.hubContext.Clients.All.ReceiveScore(score);
        }

        private void SetInput(DateTimeOffset timestamp)
        {
            var current = this.inputAction;
            this.inputAction = new();
            current.SetResult(timestamp);
        }

        private async Task StartGame()
        {
            this.game = new Game(
                Guid.NewGuid(),
                this.players.Values,
                this.RoundTypes, this.TeamCount);

            await this.hubContext.Clients.All.ReceiveTeams(this.Game.Teams);

            await Task.Delay(2000);

            this.gameLoop = this.RunGame();
        }

        private async Task RunGame()
        {
            foreach (var round in this.Game.RoundLoop())
            {
                await this.RunRound(round);
            }

            await this.hubContext.Clients.All.ReceiveResults(this.Game.GetTeamScores());
        }

        private async Task RunRound(Round round)
        {
            await this.hubContext.Clients.All.ReceiveRound(round.Type);

            foreach (var period in this.Game.PeriodLoop())
            {
                await this.RunPeriod(period);
            }
        }

        private async Task RunPeriod(Period period)
        {
            var connectionId = this.players.Keys.First(key => this.players[key].Id == period.Player.Id);

            await this.hubContext.Clients.All.ReceivePeriod(period.Player);

            var timestamp = await this.inputAction.Task;

            do
            {
                await this.hubContext.Clients.Clients(connectionId).ReceiveWord(this.Game.CurrentWord());

                timestamp = await this.inputAction.Task;
            }
            while (this.Game.NextWord(timestamp));
        }
    }
}