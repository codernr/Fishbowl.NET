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
            await base.OnConnectedAsync();
            bool askForTeamCount = this.service.Connect(this.Context.ConnectionId);

            if (askForTeamCount) await this.Clients.Caller.DefineTeamCount();
        }

        public Task SetTeamCountAsync(int teamCount)
        {
            this.service.SetTeamCount(teamCount);
            return this.Clients.Caller.DefineRoundTypes();
        }

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.service.SetRoundTypes(roundTypes);

        public async Task SetPlayerAsync(Player player)
        {
            bool isLast = this.service.SetPlayer(this.Context.ConnectionId, player);

            if (isLast) await this.Clients.All.ReceiveTeams(this.service.GameManager.Teams);
        }
    }
}