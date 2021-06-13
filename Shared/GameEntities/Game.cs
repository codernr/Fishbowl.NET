using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Game
    {
        public Guid Id { get; private set; }

        public List<Team> Teams { get; private set; }
        
        public List<Round> Rounds { get; private set; }
        
        public Round CurrentRound => this.roundEnumerator.Current;
        
        public Word CurrentWord => this.roundEnumerator.Current.WordEnumerator.Current;

        public CircularEnumerator<Team> TeamEnumerator { get; private set; }= default!;

        private readonly IEnumerator<Round> roundEnumerator = default!;

        public readonly TimeSpan periodLength;

        public readonly TimeSpan periodThreshold;

        private TimeSpan? remaining;

        public Game(Guid id, GameOptions options, IEnumerable<Team> teams, IEnumerable<string> roundTypes, bool randomize = true)
        {
            this.Id = id;

            this.periodLength = TimeSpan.FromSeconds(options.PeriodLengthInSeconds);
            this.periodThreshold = TimeSpan.FromSeconds(options.PeriodThresholdInSeconds);

            this.Teams = teams.ToList();
            this.TeamEnumerator = new CircularEnumerator<Team>(this.Teams);

            var words = this.Teams
                .SelectMany(team => team.Players)
                .SelectMany(player => player.Words);

            this.Rounds = roundTypes
                .Select(type => new Round(
                    type,
                    randomize ? new RandomEnumerator<Word>(words) : new RewindEnumerator<Word>(words)))
                .ToList();

            this.roundEnumerator = this.Rounds.GetEnumerator();
        }

        public Game(
            Guid id,
            CircularEnumerator<Team> teamEnumerator,
            List<Round> rounds,
            IEnumerator<Round> roundEnumerator,
            TimeSpan periodLength,
            TimeSpan periodThreshold) =>
            (this.Id, this.Teams, this.Rounds, this.TeamEnumerator, this.roundEnumerator, this.periodLength, this.periodThreshold) =
            (id, teamEnumerator.List.ToList(), rounds, teamEnumerator, roundEnumerator, periodLength, periodThreshold);

        public IEnumerable<Round> RoundLoop()
        {
            while(this.roundEnumerator.MoveNext())
            {
                yield return this.roundEnumerator.Current;
            }
        }

        public IEnumerable<Period> PeriodLoop()
        {
            while (this.roundEnumerator.Current.NextPeriod(
                this.remaining ?? this.periodLength,
                this.TeamEnumerator.Current.PlayerEnumerator.Current))
            {
                yield return this.roundEnumerator.Current.CurrentPeriod;
            }
        }

        public void StartPeriod(DateTimeOffset timestamp) =>
            this.roundEnumerator.Current.CurrentPeriod.StartedAt = timestamp;

        public void FinishPeriod(DateTimeOffset timestamp) =>
            this.FinishPeriod(timestamp, true);

        private void FinishPeriod(DateTimeOffset timestamp, bool rewindWord)
        {
            this.roundEnumerator.Current.CurrentPeriod.FinishedAt = timestamp;
            this.remaining = null;
            this.TeamEnumerator.Current.PlayerEnumerator.MoveNext();
            this.TeamEnumerator.MoveNext();

            if (rewindWord)
            {
                var enumerator = this.roundEnumerator.Current.WordEnumerator;
                enumerator.Return(enumerator.Current);
            }
        }

        public void AddScore(Score score) => this.roundEnumerator.Current.CurrentPeriod.Scores.Add(score);

        public Score? RevokeLastScore()
        {
            var score = this.CurrentRound.CurrentPeriod.RevokeLastScore();

            if (score is not null) this.CurrentRound.WordEnumerator.Return(score.Word);

            return score;
        }

        public bool NextWord(DateTimeOffset timestamp)
        {
            var period = this.roundEnumerator.Current.CurrentPeriod;

            if (timestamp >= period.StartedAt + period.Length - this.periodThreshold)
            {
                this.FinishPeriod(timestamp, false);
                return false;
            }

            if (!this.roundEnumerator.Current.WordEnumerator.MoveNext())
            {
                period.FinishedAt = timestamp;
                this.remaining = period.StartedAt + period.Length - timestamp;
                return false;
            }

            return true;
        }
    }
}