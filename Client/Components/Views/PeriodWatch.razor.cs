using System;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodWatch
    {
        [Parameter]
        public Round Round { get; set; } = default!;

        [Parameter]
        public Period Period { get; set; } = default!;

        private DateTimeOffset StartedAt => this.Period.StartedAt ?? throw new InvalidOperationException();
    }
}