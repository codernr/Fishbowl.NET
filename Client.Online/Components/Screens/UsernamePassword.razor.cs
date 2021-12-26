using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Store;
using Fishbowl.Net.Shared.Actions;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class UsernamePassword
    {
        private string username { get; set; } = string.Empty;

        private string password = string.Empty;

        private MudForm? form;

        private void JoinGame() => this.Dispatcher.Dispatch(new JoinGameContextAction(this.username, this.password));

        private void CreateGame() => this.Dispatcher.Dispatch(
            new SetupCreateGameContextAction(this.username, this.password));
    }
}