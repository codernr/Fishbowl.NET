using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodSetupWatch
    {
        public PeriodSetupViewModel Period { get; set; } = default!;

        protected override void SetTitle()
        {
            this.AppState.Title = this.Period.Round.Type;
        }
    }

}