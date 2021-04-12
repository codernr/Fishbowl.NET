using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class Password
    {
        [Parameter]
        public EventCallback<string> OnJoinGame { get; set; } = default!;

        [Parameter]
        public EventCallback<string> OnCreateGame { get; set; } = default!;

        private string password = string.Empty;
    }
}