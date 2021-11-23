using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class UsernamePassword
    {
        [Parameter]
        public EventCallback<GameContextJoinViewModel> OnJoinGame { get; set; } = default!;

        [Parameter]
        public EventCallback<GameContextJoinViewModel> OnCreateGame { get; set; } = default!;

        public string Username { get; set; } = string.Empty;

        private string password = string.Empty;

        private MudForm? form;
    }
}