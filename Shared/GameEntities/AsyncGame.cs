using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class AsyncGame
    {
        public event Action<Game>? GameStarted;

        public event Action<Game>? GameFinished;

        public event Action<Round>? RoundStarted;

        public event Action<Round>? RoundFinished;

        public event Action<Period>? PeriodSetup;

        public event Action<Period>? PeriodStarted;

        public event Action<Period>? PeriodFinished;

        public event Action<Score>? ScoreAdded;

        public event Action<Score>? LastScoreRevoked;

        public event Action<Player, Word>? WordSetup;

        public event Action<TimeSpan>? TimerUpdate;

        public event Action? TimerExpired;

        private TaskCompletionSource<DateTimeOffset> inputReceived = new TaskCompletionSource<DateTimeOffset>();

        private bool finishRequested = false;

        public Game Game => this.game;

        private readonly Game game;

        private Task? gameLoop;

        private Task? timer;

        private CancellationTokenSource? timerCts;

        public AsyncGame(GameOptions options, IEnumerable<Team> teams, IEnumerable<string> roundTypes, bool randomize = true) =>
            this.game = new Game(Guid.NewGuid(), options, teams, roundTypes, randomize);

        public AsyncGame(Game game) => this.game = game;

        public void Run()
        {
            this.PeriodStarted += this.StartTimer;
            this.PeriodFinished += _ => this.StopTimer();
            this.gameLoop = this.RunAsync();
        }

        public void Restore() => this.gameLoop = this.RestoreAsync();

        public void SetInput(DateTimeOffset timestamp)
        {
            var current = this.inputReceived;
            this.inputReceived = new();
            current.SetResult(timestamp);
        }

        public void StartPeriod(DateTimeOffset timestamp)
        {
            this.game.StartPeriod(timestamp);
            this.SetInput(timestamp);
        }

        public void FinishPeriod(DateTimeOffset timestamp)
        {
            this.game.FinishPeriod(timestamp);
            this.finishRequested = true;
            this.SetInput(timestamp);
        }

        public void NextWord(DateTimeOffset timestamp) => this.SetInput(timestamp);

        public void AddScore(Score score)
        {
            this.game.AddScore(score);
            this.ScoreAdded?.Invoke(score);
        }

        public void RevokeLastScore()
        {
            var score = this.Game.RevokeLastScore();

            if (score is not null) this.LastScoreRevoked?.Invoke(score);
        }

        private async Task RunAsync()
        {
            this.GameStarted?.Invoke(this.game);

            foreach (var round in this.game.RoundLoop())
            {
                await this.RunRound(round);
            }

            this.GameFinished?.Invoke(this.game);
        }

        private async Task RestoreAsync()
        {
            this.GameStarted?.Invoke(this.game);

            var currentRound = this.game.CurrentRound;

            this.RoundStarted?.Invoke(currentRound);

            var currentPeriod = currentRound.CurrentPeriod;

            if (currentPeriod.StartedAt is null)
            {
                await this.RunPeriod(currentPeriod);
            }
            else
            {
                this.PeriodStarted?.Invoke(currentPeriod);
                this.WordSetup?.Invoke(currentPeriod.Player, currentRound.WordEnumerator.Current);

                await this.GetWords(currentPeriod);
                this.PeriodFinished?.Invoke(currentPeriod);
            }

            foreach(var period in this.game.PeriodLoop()) await this.RunPeriod(period);

            this.RoundFinished?.Invoke(currentRound);

            foreach(var round in this.game.RoundLoop()) await this.RunRound(round);

            this.GameFinished?.Invoke(this.Game);
        }

        private async Task RunRound(Round round)
        {
            this.RoundStarted?.Invoke(round);

            foreach (var period in this.game.PeriodLoop())
            {
                await this.RunPeriod(period);
            }

            this.RoundFinished?.Invoke(round);
        }

        private async Task RunPeriod(Period period)
        {
            this.PeriodSetup?.Invoke(period);

            await this.inputReceived.Task;

            this.PeriodStarted?.Invoke(period);
            
            await this.GetWords(period);

            this.PeriodFinished?.Invoke(period);
        }

        private void StartTimer(Period period)
        {
            this.timerCts = new();
            this.timer = this.RunTimer(period, this.timerCts.Token);
        }

        private void StopTimer()
        {
            this.timerCts?.Cancel();
        }

        private async Task RunTimer(Period period, CancellationToken cancellationToken)
        {
            var previous = Remaining();
            var current = previous;

            while (!cancellationToken.IsCancellationRequested)
            {
                current = Remaining();

                this.TimerUpdate?.Invoke(current);

                if (current <= TimeSpan.Zero && previous > TimeSpan.Zero)
                {
                    this.TimerExpired?.Invoke();
                }
                previous = current;

                await Task.Delay(250);
            };

            TimeSpan Remaining() => period.StartedAt!.Value + period.Length - DateTimeOffset.UtcNow;
        }

        private async Task GetWords(Period period)
        {
            DateTimeOffset timestamp;

            do
            {
                this.WordSetup?.Invoke(period.Player, this.game.CurrentWord);

                timestamp = await this.inputReceived.Task;

                if (this.finishRequested)
                {
                    this.finishRequested = false;
                    break;
                }
            }
            while (this.game.NextWord(timestamp));
        }
    }
}