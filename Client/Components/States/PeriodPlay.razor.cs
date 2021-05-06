using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PeriodPlay
    {
        [Parameter]
        public EventCallback<Score> OnScoreAdded { get; set; } = default!;

        [Parameter]
        public EventCallback<DateTimeOffset> OnPeriodFinished { get; set; } = default!;

        [Parameter]
        public EventCallback OnLastScoreRevoked { get; set; } = default!;
        
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

        private bool Expired
        {
            get => this.expired;
            set
            {
                this.expired = value;
                this.Update();
            }
        }
        
        private bool expired;

        private Task ScoreAdded(EventArgs e)
        {
            var score = new Score(this.Word!, DateTimeOffset.UtcNow);
            return this.OnScoreAdded.InvokeAsync(score);
        }
    }
}