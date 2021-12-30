using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Game
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

        [JsonInclude]
        public Guid Id { get; private set; }

        [JsonInclude]
        public IGameList<Team> Teams { get; private set; } = default!;
        
        [JsonInclude]
        public IGameList<Round> Rounds { get; private set; } = default!;

        [JsonInclude]
        public GameOptions Options { get; private set; } = new();

        private TimeSpan? remaining;

        private PeriodTimer? timer;

        public Game() {}

        public Game(
            Guid id,
            GameOptions options,
            IList<Team> teams,
            IEnumerable<string> roundTypes,
            Func<IEnumerable<Word>, IRewindList<Word>> wordListFactory)
        {
            this.Id = id;
            this.Options = options;
            this.Teams = new CircularList<Team>(teams);

            var words = this.Teams.List
                .SelectMany(team => team.Players.List)
                .SelectMany(player => player.Words);

            this.Rounds = new SimpleList<Round>(roundTypes
                .Select(type => new Round(type, wordListFactory(words)))
                .ToList());
        }

        private void StartGame()
        {
            this.GameStarted?.Invoke(this);
            this.Teams.MoveNext();
            this.Teams.Current.Players.MoveNext();
            this.RunNextRound();
        }

        private void RunNextRound()
        {
            if (this.Rounds.MoveNext())
            {
                this.RoundStarted?.Invoke(this.Rounds.Current);
                this.RunNextPeriod();
            }
            else
            {
                this.GameFinished?.Invoke(this);
            }
        }

        private void RunNextPeriod()
        {
            if (this.Rounds.Current.NextPeriod(this.remaining ?? this.Options.PeriodLength, this.Teams.Current.Players.Current))
            {
                this.PeriodSetup?.Invoke(this.Rounds.Current.CurrentPeriod);
            }
            else
            {
                this.RoundFinished?.Invoke(this.Rounds.Current);
                this.RunNextRound();
            }
        }

        public void StartPeriod(DateTimeOffset timestamp)
        {
            var period = this.Rounds.Current.CurrentPeriod;

            period.StartedAt = timestamp;
            this.PeriodStarted?.Invoke(period);

            this.timer = new PeriodTimer(
                this.TimerUpdate, period.StartedAt!.Value, period.Length);

            this.NextWord(timestamp);
            this.WordSetup?.Invoke(period.Player, this.Rounds.Current.Words.Current);
        }

        public void AddScore(Score score)
        {
            this.ScoreAdded?.Invoke(score);

            this.NextWord(score.Timestamp);
        }

        private void NextWord(DateTimeOffset timestamp)
        {
            var period = this.Rounds.Current.CurrentPeriod;

            if (timestamp >= period.StartedAt + period.Length - this.Options.PeriodThreshold)
            {
                this.FinishPeriod(timestamp, true, false);
                return;
            }

            if (!this.Rounds.Current.Words.MoveNext())
            {
                this.FinishPeriod(timestamp, false, false, period.StartedAt + period.Length - timestamp);
                return;
            }

            this.WordSetup?.Invoke(period.Player, this.Rounds.Current.Words.Current);

        }

        public void FinishPeriod(DateTimeOffset timestamp) => this.FinishPeriod(timestamp, true, true);

        private void FinishPeriod(DateTimeOffset timestamp, bool newPlayer, bool rewind, TimeSpan? remaining = null)
        {
            this.Rounds.Current.CurrentPeriod.FinishedAt = timestamp;
            this.remaining = remaining;
            
            if (newPlayer)
            {
                this.Teams.MoveNext();
                this.Teams.Current.Players.MoveNext();
            }

            if (rewind)
            {
                this.Rounds.Current.Words.MovePrevious();
            }

            this.PeriodFinished?.Invoke(this.Rounds.Current.CurrentPeriod);

            this.RunNextPeriod();
        }

        public void RevokeLastScore()
        {
            var score = this.Rounds.Current.CurrentPeriod.RevokeLastScore();

            if (score is not null)
            {
                this.Rounds.Current.Words.MovePrevious();
                this.LastScoreRevoked?.Invoke(score);
            }
        }

        public void Restore()
        {
            this.GameStarted?.Invoke(this);

            this.RoundStarted?.Invoke(this.Rounds.Current);

            var period = this.Rounds.Current.CurrentPeriod;

            if (period.StartedAt is null)
            {
                this.PeriodSetup?.Invoke(period);
            }
            else
            {
                this.PeriodStarted?.Invoke(period);
                this.WordSetup?.Invoke(period.Player, this.Rounds.Current.Words.Current);
            }
        }
    }
}