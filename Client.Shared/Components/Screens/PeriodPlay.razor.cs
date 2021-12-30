using System;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class PeriodPlay
    {
        public PeriodRunningViewModel Period => this.State.Value.Current.Running;

        protected override string Title => this.Period.Round.Type;

        private bool Expired => this.State.Value.Current.Remaining < TimeSpan.Zero;

        private void AddScore() =>
            this.Dispatcher.Dispatch(new AddScoreAction(this.State.Value.Current.Word!));

        private void Revoke() => this.Dispatcher.Dispatch(new RevokeLastScoreAction());

        private void FinishPeriod() => this.DispatchOnce<FinishPeriodAction>(new());
    }
}