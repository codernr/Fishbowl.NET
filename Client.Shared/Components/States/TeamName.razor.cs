using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public TeamViewModel Team { get; set; } = default!;

        private string Value { get; set; } = string.Empty;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        [Parameter]
        public EventCallback<TeamNameViewModel> OnTeamNameSet { get; set; } = default!;

        private bool isValid;

        public void Reset()
        {
            this.Value = string.Empty;
            this.IsValid = false;
        }
    }
}