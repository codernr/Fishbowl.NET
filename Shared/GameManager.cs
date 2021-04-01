using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared
{
    public class GameManager
    {
        private readonly Game game;

        public Game Game => this.game;

        public Word CurrentWord { get => this.game.Rounds.Current.Words.Current; }

        public IEnumerable<Team> Teams { get; private set; }

        public IEnumerator<Round> Rounds => this.game.Rounds;

        public IEnumerator<Period> Periods => this.GetPeriods().GetEnumerator();

        public GameManager(Guid id, IEnumerable<Player> players, IEnumerable<string> roundTypes, int teamCount, bool randomize = true)
        {
            var randomPlayersList = (randomize ? players.Randomize() : players).ToList();

            if (randomPlayersList.Count < teamCount * 2)
            {
                throw new ArgumentException("Player count should be at least (team count) * 2.");
            }

            var words = randomPlayersList
                .SelectMany(player => player.Words);

            this.Teams = randomPlayersList
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()));

            var rounds = roundTypes
                .Select(type => new Round(
                    type,
                    randomize ? new RandomEnumerator<Word>(words) : new RewindEnumerator<Word>(words)));

            this.game = new Game(id, this.Teams, rounds);
        }

        public IEnumerable<Round> GetRounds()
        {
            while(this.game.Rounds.MoveNext())
            {
                yield return this.game.Rounds.Current;
            }
        }

        public IEnumerable<Period> GetPeriods()
        {
            while (this.game.Rounds.Current.NextPeriod(this.game.Remaining ?? this.game.PeriodLength, this.game.Teams.Current.PlayerEnumerator.Current))
            {
                yield return this.game.Rounds.Current.CurrentPeriod;
            }
        }

        public bool NextWord(DateTimeOffset timestamp, Word? previousWord = null)
        {
            var period = this.game.Rounds.Current.CurrentPeriod;

            if (period.StartedAt is null)
            {
                period.StartedAt = timestamp;
                return true;
            }

            if (previousWord is not null)
            {
                period.Scores.Add(new Score(previousWord, timestamp));
            }

            if (timestamp >= period.StartedAt + period.Length - Game.PeriodThreshold)
            {
                period.FinishedAt = timestamp;
                this.game.Remaining = null;
                this.game.Teams.Current.PlayerEnumerator.MoveNext();
                this.game.Teams.MoveNext();

                if (previousWord is null) this.game.Rounds.Current.Words.MovePrevious();
                return false;
            }

            if (!this.game.Rounds.Current.Words.MoveNext())
            {
                period.FinishedAt = timestamp;
                this.game.Remaining = period.StartedAt + period.Length - timestamp;
                return false;
            }

            return true;
        }
    }
}