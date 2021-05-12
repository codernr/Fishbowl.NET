using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fishbowl.Net.Client
{
    public static class HubConnectionExtensions
    {
        public static Task<bool> GameContextExists(this HubConnection hubConnection, string password) =>
            hubConnection.InvokeAsync<bool>("GameContextExists", password);

        public static Task<bool> JoinGameContext(this HubConnection hubConnection, GameContextJoinViewModel gameContextJoin) =>
            hubConnection.InvokeAsync<bool>("JoinGameContext", gameContextJoin);

        public static Task CreateGameContext(this HubConnection hubConnection, GameContextSetupViewModel gameContextSetup) =>
            hubConnection.InvokeAsync("CreateGameContext", gameContextSetup);

        public static Task AddPlayer(this HubConnection hubConnection, Player player) =>
            hubConnection.InvokeAsync("AddPlayer", player);
    }
}