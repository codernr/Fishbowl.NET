using System;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.Data
{
    public class Game
    { 
        private readonly CircularEnumerator<Team> teamEnumerator;

        private readonly IEnumerator<Round> roundEnumerator;

        public Guid Id { get; private set; }

        public IList<Team> Teams { get; private set; }
        
        public IList<Round> Rounds { get; private set; }
        
        public TimeSpan? Remaining { get; set; }

        public TimeSpan PeriodLength { get; private set; } = TimeSpan.FromSeconds(60);

        public TimeSpan PeriodThreshold { get; private set; } = TimeSpan.FromSeconds(5);

        public Game(
            Guid id,
            IEnumerable<Player> players,
            IEnumerable<string> roundTypes,
            int teamCount = 2,
            bool randomize = true)
        {
            this.Id = id;

            var playerList = (randomize ? players.Randomize() : players).ToList();

            if (playerList.Count < teamCount * 2)
            {
                throw new ArgumentException("Player count should be at least (team count) * 2.");
            }

            this.Teams = players
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()))
                .ToList();

            this.teamEnumerator = new CircularEnumerator<Team>(this.Teams);

            var words = players
                .SelectMany(player => player.Words);

            this.Rounds = roundTypes
                .Select(type => new Round(
                    type,
                    randomize ? new RandomEnumerator<Word>(words) : new RewindEnumerator<Word>(words)))
                .ToList();

            this.roundEnumerator = this.Rounds.GetEnumerator();
        }

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
                this.Remaining ?? this.PeriodLength,
                this.teamEnumerator.Current.PlayerEnumerator().Current))
            {
                yield return this.roundEnumerator.Current.CurrentPeriod();
            }
        }

        public Word CurrentWord() => this.roundEnumerator.Current.WordEnumerator().Current;

        public bool NextWord(DateTimeOffset timestamp, Word? previousWord = null)
        {
            var period = this.roundEnumerator.Current.CurrentPeriod();

            if (period.StartedAt is null)
            {
                period.StartedAt = timestamp;
                return true;
            }

            if (previousWord is not null)
            {
                period.Scores.Add(new Score(previousWord, timestamp));
            }

            if (timestamp >= period.StartedAt + period.Length - this.PeriodThreshold)
            {
                period.FinishedAt = timestamp;
                this.Remaining = null;
                this.teamEnumerator.Current.PlayerEnumerator().MoveNext();
                this.teamEnumerator.MoveNext();

                if (previousWord is null) this.roundEnumerator.Current.WordEnumerator().MovePrevious();
                return false;
            }

            if (!this.roundEnumerator.Current.WordEnumerator().MoveNext())
            {
                period.FinishedAt = timestamp;
                this.Remaining = period.StartedAt + period.Length - timestamp;
                return false;
            }

            return true;
        }
    }
}