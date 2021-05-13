using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class TeamName
    {
        [Parameter]
        public TeamViewModel Team { get; set; } = default!;

        [Parameter]
        public EventCallback<string> OnTeamNameSet { get; set; } = default!;

        private string teamName = string.Empty;
    }
}