using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodSetupWatch
    {
        public PeriodSetupViewModel Period => this.State.Value.Period;

        protected override string Title => this.Period.Round.Type;
    }

}