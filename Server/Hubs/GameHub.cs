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

        public override async Task OnConnectedAsync()
        {
            var connections = this.service.RegisterConnection(this.Context.ConnectionId);

            if (connections == 1) await this.Clients.Caller.DefineTeamCount();

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            this.service.RemoveConnection(this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SetTeamCountAsync(int teamCount)
        {
            this.service.SetTeamCount(teamCount);
            return this.Clients.Caller.DefineRoundTypes();
        }

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.service.SetRoundTypes(roundTypes);

        public Task SetPlayerAsync(Player player) =>
            this.service.SetPlayerAsync(this.Context.ConnectionId, player);

        public Task StartPeriodAsync(DateTimeOffset timestamp) => this.service.StartPeriodAsync(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.service.NextWord(timestamp);

        public Task AddScoreAsync(Score score) => this.service.AddScoreAsync(score);

        public void FinishPeriod(DateTimeOffset timestamp) => this.service.FinishPeriod(timestamp);
    }
}