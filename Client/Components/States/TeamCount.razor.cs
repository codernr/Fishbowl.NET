using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class TeamCount
    {
        [Parameter]
        public EventCallback<int> OnTeamCountSet { get; set; } = default!;

        public int MaxTeamCount
        {
            get => this.maxTeamCount;
            set
            {
                this.maxTeamCount = value;
                this.Update();
            }
        }

        private int maxTeamCount = 2;

        private int teamCount = 2;
    }
}