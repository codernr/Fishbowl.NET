using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodSetupPlay
    {
        private PeriodSetupViewModel Period => this.State.Value.Period!;

        protected override string Title => $"{this.Period.Round.Type}: {this.Period.Player.Username}";

        private void Start() => this.Dispatcher.Dispatch(new StartPeriodAction());
    }
}