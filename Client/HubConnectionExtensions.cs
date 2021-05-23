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

        public static Task<StatusResponse> JoinGameContext(this HubConnection hubConnection, GameContextJoinViewModel gameContextJoin) =>
            hubConnection.InvokeAsync<StatusResponse>("JoinGameContext", gameContextJoin);

        public static Task<StatusResponse> CreateGameContext(this HubConnection hubConnection, GameContextSetupViewModel gameContextSetup) =>
            hubConnection.InvokeAsync<StatusResponse>("CreateGameContext", gameContextSetup);

        public static Task<StatusResponse> AddPlayer(this HubConnection hubConnection, Player player) =>
            hubConnection.InvokeAsync<StatusResponse>("AddPlayer", player);

        public static Task<StatusResponse> SetTeamName(this HubConnection hubConnection, TeamNameViewModel teamName) =>
            hubConnection.InvokeAsync<StatusResponse>("SetTeamName", teamName);
    }
}