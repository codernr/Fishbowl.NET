using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared
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

        private TaskCompletionSource<DateTimeOffset> inputReceived = new TaskCompletionSource<DateTimeOffset>();

        private bool finishRequested = false;

        public Game Game => this.game;

        private readonly Game game;

        private Task? gameLoop;

        public AsyncGame(IEnumerable<Team> teams, IEnumerable<string> roundTypes, bool randomize = true) =>
            this.game = new Game(Guid.NewGuid(), teams, roundTypes, randomize);

        public void Run() => this.gameLoop = this.RunAsync();

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

            var timestamp = await this.inputReceived.Task;

            this.PeriodStarted?.Invoke(period);
            
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

            this.PeriodFinished?.Invoke(period);
        }
    }
}