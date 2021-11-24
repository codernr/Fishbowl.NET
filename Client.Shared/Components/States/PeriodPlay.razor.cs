using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodPlay
    {
        public Action<ScoreViewModel> OnScoreAdded { get; set; } = default!;

        public Action<DateTimeOffset> OnPeriodFinished { get; set; } = default!;

        public Action OnLastScoreRevoked { get; set; } = default!;
        
        public PeriodRunningViewModel? Period { get; set; }

        public bool Expired
        {
            get => this.expired;
            set
            {
                this.expired = value;
                this.StateHasChanged();
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

        private void ScoreAdded(EventArgs e)
        {
            var score = new ScoreViewModel(this.Word!, DateTimeOffset.UtcNow);
            this.OnScoreAdded(score);
        }
    }
}