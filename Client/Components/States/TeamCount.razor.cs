using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class TeamCount
    {
        [Parameter]
        public EventCallback<int> OnTeamCountSet { get; set; } = default!;

        private int teamCount = 2;
    }
}