using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameService service;

        public GameHub(GameService service) => this.service = service;

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await this.service.JoinGameAsync(this.Context.ConnectionId, this.Clients.Caller);
        }

        public Task SetTeamCountAsync(int teamCount) => this.service.SetTeamCountAsync(this.Clients.Caller, teamCount);

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.service.SetRoundTypes(roundTypes);

        public Task SetPlayerAsync(string name, IEnumerable<string> words) =>
            this.service.SetPlayerAsync(this.Context.ConnectionId, name, words.Select(w => new Word(Guid.NewGuid(), w)));
    }
}