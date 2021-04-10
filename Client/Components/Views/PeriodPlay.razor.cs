using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodPlay
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

        public Word? Word
        {
            get => this.word;
            set
            {
                this.word = value;
                this.StateHasChanged();
            }
        }

        private Word? word;

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