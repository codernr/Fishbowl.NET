using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly GameService service;

        public GameHub(GameService service) => this.service = service;

        private GameContext GameContext => this.service.GetContext(this.Context.ConnectionId);

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await this.service.RemoveConnection(this.Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public Task CreateGame(string password) => this.service.CreateGame(this.Context.ConnectionId, password);

        public Task JoinGame(string password) => this.service.JoinGame(this.Context.ConnectionId, password);

        public void SetTeamCount(int teamCount) => this.GameContext.Game.SetTeamCount(teamCount);

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.GameContext.SetRoundTypes(roundTypes);

        public void AddPlayer(Player player) =>
            this.GameContext.AddPlayer(this.Context.ConnectionId, player);

        public void StartPeriod(DateTimeOffset timestamp) => this.GameContext.Game.StartPeriod(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.GameContext.Game.NextWord(timestamp);

        public void AddScore(Score score) => this.GameContext.Game.AddScore(score);

        public void FinishPeriod(DateTimeOffset timestamp) => this.GameContext.Game.FinishPeriod(timestamp);
    }
}