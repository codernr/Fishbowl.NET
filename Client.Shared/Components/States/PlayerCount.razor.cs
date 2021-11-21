using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerCount
    {
        [Parameter]
        public EventCallback<int> OnPlayerCountSet { get; set; } = default!;

        private int playerCount = 4;
    }
}