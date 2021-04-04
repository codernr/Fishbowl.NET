using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Hubs
{
    public class GameHub : Hub<IClient>
    {
        private readonly GameService service;

        public GameHub(GameService service) => this.service = service;

        public override Task OnConnectedAsync()
        {
            this.service.RegisterConnection(this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            this.service.RemoveConnection(this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void SetTeamCount(int teamCount) => this.service.Game.SetTeamCount(teamCount);

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.service.Game.SetRoundTypes(roundTypes);

        public void AddPlayer(Player player) =>
            this.service.AddPlayer(this.Context.ConnectionId, player);

        public void StartPeriod(DateTimeOffset timestamp) => this.service.Game.StartPeriod(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.service.Game.NextWord(timestamp);

        public void AddScore(Score score) => this.service.Game.AddScore(score);

        public void FinishPeriod(DateTimeOffset timestamp) => this.service.Game.FinishPeriod(timestamp);
    }
}