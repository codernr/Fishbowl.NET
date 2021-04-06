using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared
{
    public class AsyncGame
    {
        public event Action? WaitingForTeamCount;

        public event Action? WaitingForRoundTypes;

        public event Action<Game>? GameStarted;

        public event Action<Game>? GameFinished;

        public event Action<Round>? RoundStarted;

        public event Action<Round>? RoundFinished;

        public event Action<Period>? PeriodSetup;

        public event Action<Period>? PeriodStarted;

        public event Action<Period>? PeriodFinished;

        public event Action<Score>? ScoreAdded;

        public event Action<Player, Word>? WordSetup;

        private readonly List<Player> players = new List<Player>();

        private readonly TaskCompletionSource playersSet = new TaskCompletionSource();

        private readonly TaskCompletionSource teamCountSet = new TaskCompletionSource();

        private readonly TaskCompletionSource roundTypesSet = new TaskCompletionSource();

        private TaskCompletionSource<DateTimeOffset> inputReceived = new TaskCompletionSource<DateTimeOffset>();

        private bool finishRequested = false;

        private Game? game;

        private Game Game => this.game ??
            throw new InvalidOperationException("Invalid game state: GameManager is not defined");

        private int? teamCount;

        private IEnumerable<string>? roundTypes;

        private readonly bool randomize;

        private Task? gameLoop;

        public AsyncGame(bool randomize = true) => this.randomize = randomize;

        public void SetTeamCount(int teamCount)
        {
            this.teamCount = teamCount;
            this.teamCountSet.SetResult();
            this.WaitingForRoundTypes?.Invoke();
        }

        public void SetRoundTypes(IEnumerable<string> roundTypes)
        {
            this.roundTypes = roundTypes;
            this.roundTypesSet.SetResult();
        }

        public void AddPlayer(Player player) => this.players.Add(player);

        public void PlayersSet() => this.playersSet.SetResult();

        public void SetInput(DateTimeOffset timestamp)
        {
            var current = this.inputReceived;
            this.inputReceived = new();
            current.SetResult(timestamp);
        }

        public void StartPeriod(DateTimeOffset timestamp)
        {
            this.Game.StartPeriod(timestamp);
            this.SetInput(timestamp);
        }

        public void FinishPeriod(DateTimeOffset timestamp)
        {
            this.Game.FinishPeriod(timestamp);
            this.finishRequested = true;
            this.SetInput(timestamp);
        }

        public void NextWord(DateTimeOffset timestamp) => this.SetInput(timestamp);

        public void AddScore(Score score)
        {
            this.Game.AddScore(score);
            this.ScoreAdded?.Invoke(score);
        }

        public void Run() => this.gameLoop = this.RunAsync();
        
        private async Task RunAsync()
        {
            this.WaitingForTeamCount?.Invoke();

            await Task.WhenAll(this.playersSet.Task, this.roundTypesSet.Task, this.teamCountSet.Task);

            if (this.roundTypes is null || this.teamCount is null)
            {
                throw new InvalidOperationException("Round types or team count is not set");
            }

            this.game = new Game(Guid.NewGuid(), this.players, this.roundTypes, this.teamCount.Value, this.randomize);

            this.GameStarted?.Invoke(this.game);

            foreach (var round in this.Game.RoundLoop())
            {
                await this.RunRound(round);
            }

            this.GameFinished?.Invoke(this.game);
        }

        private async Task RunRound(Round round)
        {
            this.RoundStarted?.Invoke(round);

            foreach (var period in this.Game.PeriodLoop())
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
                this.WordSetup?.Invoke(period.Player, this.Game.CurrentWord());

                timestamp = await this.inputReceived.Task;

                if (this.finishRequested)
                {
                    this.finishRequested = false;
                    break;
                }
            }
            while (this.Game.NextWord(timestamp));

            this.PeriodFinished?.Invoke(period);
        }
    }
}