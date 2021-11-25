using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodWatch
    {
        public PeriodRunningViewModel Period { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.AppState.Title = this.Period.Round.Type;
        }
    }
}