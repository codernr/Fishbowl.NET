using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared;
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

        private TaskCompletionSource<(DateTimeOffset, Word?)> input = new();

        private int? teamCount;

        private IEnumerable<string>? roundTypes;

        private GameManager? gameManager;

        private Task? gameLoop;

        private IEnumerable<string> RoundTypes => this.roundTypes ??
            throw new InvalidOperationException("Invalid game state: RoundTypes are not defined");

        private int TeamCount => this.teamCount ??
            throw new InvalidOperationException("Invalid game state: TeamCount is not defined");

        private GameManager GameManager => this.gameManager ??
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

        public void SetInput(DateTimeOffset timestamp, Word? word)
        {
            var current = this.input;
            this.input = new();
            current.SetResult((timestamp, word));
        }

        private async Task StartGame()
        {
            this.gameManager = new GameManager(
                Guid.NewGuid(),
                this.players.Values,
                this.RoundTypes, this.TeamCount);

            await this.hubContext.Clients.All.ReceiveTeams(
                this.GameManager.Game.Teams
                .Select(team => new TeamViewModel(team.Id, team.Players
                    .Select(player => player.Id))));

            await Task.Delay(2000);

            this.gameLoop = this.RunGame();
        }

        private async Task RunGame()
        {
            foreach (var round in this.GameManager.GetRounds())
            {
                await this.RunRound(round);
            }

            await this.FinishGame();
        }

        private async Task RunRound(Round round)
        {
            await this.hubContext.Clients.All.ReceiveRound(round.Type);

            foreach (var period in this.GameManager.GetPeriods())
            {
                await this.RunPeriod(period);
            }
        }

        private async Task RunPeriod(Period period)
        {
            var connectionId = this.players.Keys.First(key => this.players[key].Id == period.Player.Id);

            await this.hubContext.Clients.Clients(connectionId).ReceivePeriod(period.Player);

            var (timestamp, guessedWord) = await this.input.Task;

            while(this.GameManager.NextWord(timestamp, guessedWord))
            {
                if (guessedWord is not null)
                {
                    await this.hubContext.Clients.AllExcept(connectionId).ReceiveScore(period.Scores.Last());
                }

                await this.hubContext.Clients.Clients(connectionId).ReceiveWord(this.GameManager.CurrentWord);

                (timestamp, guessedWord) = await this.input.Task;
            }
        }

        private Task FinishGame()
        {
            var playerScores = this.GameManager.Game.Rounds
                .SelectMany(round => round.Periods)
                .GroupBy(period => period.Player.Id)
                .ToDictionary(item => item.Key, item => item.Count());

            var teamScores = this.GameManager.Game.Teams
                .ToDictionary(
                    team => team.Id,
                    team => playerScores
                        .Where(score => team.Players
                            .Any(player => player.Id == score.Key))
                        .Count());

            return this.hubContext.Clients.All.ReceiveResults(teamScores);
        }
    }
}