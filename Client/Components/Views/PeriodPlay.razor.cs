using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodPlay
    {
        [Parameter]
        public Round Round { get; set; } = default!;

        [Parameter]
        public Period Period { get; set; } = default!;

        [Parameter]
        public Word? Word { get; set; }

        [Parameter]
        public EventCallback<Score> OnScoreAdded { get; set; } = default!;

        [Parameter]
        public EventCallback<DateTimeOffset> OnPeriodFinished { get; set; } = default!;

        private DateTimeOffset StartedAt => this.Period.StartedAt ?? throw new InvalidOperationException();

        private bool expired;

        private Task ScoreAdded(EventArgs e)
        {
            var score = new Score(this.Word!, DateTimeOffset.UtcNow);
            return this.OnScoreAdded.InvokeAsync(score);
        }
    }
}