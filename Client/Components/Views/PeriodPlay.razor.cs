using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodPlay
    {
        [Parameter]
        public EventCallback<Score> OnScoreAdded { get; set; } = default!;

        [Parameter]
        public EventCallback<DateTimeOffset> OnPeriodFinished { get; set; } = default!;
        
        public Round Round { get; set; } = default!;

        public Period Period { get; set; } = default!;

        public Word? Word
        {
            get => this.word;
            set
            {
                this.word = value;
                this.Update();
            }
        }

        private Word? word;

        private DateTimeOffset StartedAt => this.Period.StartedAt ?? throw new InvalidOperationException();

        private bool expired;

        private Task ScoreAdded(EventArgs e)
        {
            var score = new Score(this.Word!, DateTimeOffset.UtcNow);
            return this.OnScoreAdded.InvokeAsync(score);
        }
    }
}