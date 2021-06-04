using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class TeamName
    {
        public TeamViewModel Team { get; set; } = default!;

        [Parameter]
        public EventCallback<TeamNameViewModel> OnTeamNameSet { get; set; } = default!;

        private string teamName = string.Empty;
    }
}