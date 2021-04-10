using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodWatch
    {
        public Round Round { get; set; } = default!;

        public Period Period { get; set; } = default!;

        private DateTimeOffset StartedAt => this.Period.StartedAt ?? throw new InvalidOperationException();
    }
}