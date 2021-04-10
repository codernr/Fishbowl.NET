using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodFinished
    {
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
    }
}