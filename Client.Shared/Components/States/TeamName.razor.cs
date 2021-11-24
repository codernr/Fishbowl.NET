using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public Func<TeamNameViewModel, Task> OnTeamNameSet { get; set; } = default!;

        public TeamViewModel Team { get; set; } = default!;

        private MudForm? form;

        private string teamName = string.Empty;
    }
}