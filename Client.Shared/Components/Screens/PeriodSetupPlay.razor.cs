using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class PeriodSetupPlay
    {
        private PeriodSetupViewModel Period => this.State.Value.Setup;

        protected override string Title => $"{this.Period.Round.Type}: {this.Period.Player.Username}";

        private void Start() => this.DispatchOnce<StartPeriodAction>(new());
    }
}