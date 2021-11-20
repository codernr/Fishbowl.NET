using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerCount
    {
        [Parameter]
        public EventCallback<int> OnPlayerCountSet { get; set; } = default!;

        public MudForm? form;

        private int playerCount = 4;
    }
}