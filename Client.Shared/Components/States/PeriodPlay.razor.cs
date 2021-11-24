using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodPlay
    {
        public Func<ScoreViewModel, Task> OnScoreAdded { get; set; } = default!;

        public Func<DateTimeOffset, Task> OnPeriodFinished { get; set; } = default!;

        public Func<Task> OnLastScoreRevoked { get; set; } = default!;
        
        public PeriodRunningViewModel Period { get; set; } = default!;

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

        private Once once = new();

        protected override void SetTitle()
        {
            this.AppState.Title = this.Period.Round.Type;
        }

        private Task AddScore()
        {
            var score = new ScoreViewModel(this.Word!, DateTimeOffset.UtcNow);
            return this.OnScoreAdded(score);
        }

        private Task FinishPeriod() =>
            this.once.Fire(() => this.OnPeriodFinished(DateTimeOffset.UtcNow));
    }
}