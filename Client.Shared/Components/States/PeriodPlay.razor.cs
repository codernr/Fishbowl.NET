using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodPlay
    {
        [Parameter]
        public EventCallback<ScoreViewModel> OnScoreAdded { get; set; } = default!;

        [Parameter]
        public EventCallback<DateTimeOffset> OnPeriodFinished { get; set; } = default!;

        [Parameter]
        public EventCallback OnLastScoreRevoked { get; set; } = default!;
        
        public PeriodRunningViewModel Period { get; set; } = default!;

        public bool Expired
        {
            get => this.expired;
            set
            {
                this.expired = value;
                this.Update();
            }
        }
        
        private bool expired;

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

        public WordViewModel? Word { get; set; }

        private bool showRevoke = false;

        private Task ScoreAdded(EventArgs e)
        {
            var score = new ScoreViewModel(this.Word!, DateTimeOffset.UtcNow);
            return this.OnScoreAdded.InvokeAsync(score);
        }
    }
}