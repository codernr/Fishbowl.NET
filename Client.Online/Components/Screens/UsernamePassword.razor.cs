using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Store;
using Fishbowl.Net.Shared.Actions;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class UsernamePassword
    {
        public Func<JoinGameContextAction, Task> OnJoinGame { get; set; } = default!;

        public Func<SetupCreateGameContextAction, Task> OnCreateGame { get; set; } = default!;

        public string Username { get; set; } = string.Empty;

        private string password = string.Empty;

        private MudForm? form;
    }
}