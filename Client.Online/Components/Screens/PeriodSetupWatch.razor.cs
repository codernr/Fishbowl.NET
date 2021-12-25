using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PeriodSetupWatch
    {
        public PeriodSetupViewModel Period => this.State.Value.Setup;

        protected override string Title => this.Period.Round.Type;
    }

}