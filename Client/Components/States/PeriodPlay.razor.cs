using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
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
        
        public PeriodRunningViewModel Period { get; set; } = default!;

        public int ScoreCount
        {
            get => this.scoreCount;
            set
            {
                this.showRevoke = value > this.scoreCount;
                this.scoreCount = value;
            }
        }

        private int scoreCount = 0;

        public Word? Word { get; set; }

        private bool showRevoke = false;

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