using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PeriodSetupWatch
    {
        protected override string Title => this.State.Value.Setup.Round.Type;
    }

}