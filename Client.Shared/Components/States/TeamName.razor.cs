using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class TeamName
    {
        public Func<TeamNameViewModel, Task> OnTeamNameSet { get; set; } = default!;

        public TeamViewModel Team { get; set; } = default!;

        private string Value { get; set; } = string.Empty;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.StateHasChanged();
            }
        }

        private bool isValid;

        public void Reset()
        {
            this.Value = string.Empty;
            this.IsValid = false;
        }
    }
}