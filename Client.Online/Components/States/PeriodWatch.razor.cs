using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodWatch
    {
        public PeriodRunningViewModel Period { get; set; } = default!;

        public bool ShowTeamAlert { get; set; } = false;

        private bool expired = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.AppState.Title = this.Period.Round.Type;
        }
    }
}