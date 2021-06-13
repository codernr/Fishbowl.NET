using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public TeamViewModel Team { get; set; } = default!;

        public string Value { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<TeamNameViewModel> OnTeamNameSet { get; set; } = default!;
    }
}