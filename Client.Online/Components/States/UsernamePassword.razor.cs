using System;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class UsernamePassword
    {
        [Parameter]
        public EventCallback<(string, string)> OnJoinGame { get; set; } = default!;

        [Parameter]
        public EventCallback<(string, string)> OnCreateGame { get; set; } = default!;

        public string Username { get; set; } = string.Empty;

        private string password = string.Empty;

        private MudForm? form;
    }
}