using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class TeamName
    {
        public TeamViewModel Team { get; set; } = default!;

        [Parameter]
        public EventCallback<TeamNameViewModel> OnTeamNameSet { get; set; } = default!;

        private string teamName = string.Empty;
    }
}