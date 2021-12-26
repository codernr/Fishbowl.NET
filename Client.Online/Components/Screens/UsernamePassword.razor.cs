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

        private void JoinGame() => this.DispatchOnce<JoinGameContextAction>(new(this.password, this.username));

        private void CreateGame() => this.DispatchOnce<SetupCreateGameContextAction>(new(this.username, this.password));
    }
}