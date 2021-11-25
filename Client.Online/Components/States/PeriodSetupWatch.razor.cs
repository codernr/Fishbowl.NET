using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodSetupWatch
    {
        public PeriodSetupViewModel Period { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Title = this.Period.Round.Type;
        }
    }

}