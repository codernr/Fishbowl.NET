using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Shared.ViewModels;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public Func<TeamNameViewModel, Task> OnTeamNameSet { get; set; } = default!;

        public TeamViewModel Team { get; set; } = default!;

        public string Title = default!;

        private MudForm? form;

        private Once once = new();

        private string teamName = string.Empty;

        protected override void SetTitle()
        {
            this.AppState.Title = this.Title;
        }

        private Task Submit() =>
            this.once.Fire(() => this.OnTeamNameSet(new(this.Team.Id, this.teamName)));
    }
}