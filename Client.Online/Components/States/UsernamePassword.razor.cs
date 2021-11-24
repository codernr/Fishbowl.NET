using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class UsernamePassword
    {
        public Func<GameContextJoinViewModel, Task> OnJoinGame { get; set; } = default!;

        public Func<GameContextJoinViewModel, Task> OnCreateGame { get; set; } = default!;

        public string Username { get; set; } = string.Empty;

        private string password = string.Empty;

        private MudForm? form;
    }
}