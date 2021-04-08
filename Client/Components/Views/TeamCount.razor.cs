using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class TeamCount
    {
        [Parameter]
        public EventCallback<int> OnTeamCountSet { get; set; } = default!;

        private int teamCount = 2;
    }
}