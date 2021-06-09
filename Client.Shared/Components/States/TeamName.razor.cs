using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public TeamViewModel Team { get; set; } = default!;

        public string Value { get; set; } = string.Empty;

        private bool submitted = false;

        [Parameter]
        public EventCallback<TeamNameViewModel> OnTeamNameSet { get; set; } = default!;

        public override Task EnableAsync()
        {
            this.submitted = false;
            this.Update();
            return base.EnableAsync();
        }

        private Task Submit()
        {
            if (this.submitted) return Task.CompletedTask;
            
            this.submitted = true;

            return this.OnTeamNameSet.InvokeAsync(new(this.Team.Id, this.Value));
        }
    }
}