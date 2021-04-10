using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodWatch
    {
        public Round Round
        {
            get => this.round ?? throw new InvalidOperationException();
            set
            {
                this.round = value;
                this.StateHasChanged();
            }
        }

        private Round? round;

        public Period Period
        {
            get => this.period ?? throw new InvalidOperationException();
            set
            {
                this.period = value;
                this.StateHasChanged();
            }
        }

        private Period? period;

        private DateTimeOffset StartedAt => this.Period.StartedAt ?? throw new InvalidOperationException();
    }
}