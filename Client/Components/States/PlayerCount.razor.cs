using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PlayerCount
    {
        [Parameter]
        public EventCallback<int> OnPlayerCountSet { get; set; } = default!;

        private int playerCount = 2;
    }
}