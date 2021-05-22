using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fishbowl.Net.Client
{
    public static class HubConnectionExtensions
    {
        public static Task<bool> GameContextExists(this HubConnection hubConnection, string password) =>
            hubConnection.InvokeAsync<bool>("GameContextExists", password);

        public static Task<StatusCode> JoinGameContext(this HubConnection hubConnection, GameContextJoinViewModel gameContextJoin) =>
            hubConnection.InvokeAsync<StatusCode>("JoinGameContext", gameContextJoin);

        public static Task<StatusCode> CreateGameContext(this HubConnection hubConnection, GameContextSetupViewModel gameContextSetup) =>
            hubConnection.InvokeAsync<StatusCode>("CreateGameContext", gameContextSetup);

        public static Task AddPlayer(this HubConnection hubConnection, Player player) =>
            hubConnection.InvokeAsync("AddPlayer", player);

        public static Task SetTeamName(this HubConnection hubConnection, TeamNameViewModel teamName) =>
            hubConnection.InvokeAsync("SetTeamName", teamName);
    }
}