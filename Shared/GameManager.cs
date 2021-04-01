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

        public Word CurrentWord { get => this.game.RoundEnumerator.Current.Words.Current; }

        public GameManager(Guid id, IEnumerable<Player> players, IEnumerable<string> roundTypes, int teamCount, bool randomize = true)
        {
            var randomPlayersList = (randomize ? players.Randomize() : players).ToList();

            if (randomPlayersList.Count < teamCount * 2)
            {
                throw new ArgumentException("Player count should be at least (team count) * 2.");
            }

            var words = randomPlayersList
                .SelectMany(player => player.Words);

            var teams = randomPlayersList
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()));

            var rounds = roundTypes
                .Select(type => new Round(
                    type,
                    randomize ? new RandomEnumerator<Word>(words) : new RewindEnumerator<Word>(words)));

            this.game = new Game(id, teams, rounds);
        }

        public IEnumerable<Round> GetRounds()
        {
            while(this.game.RoundEnumerator.MoveNext())
            {
                yield return this.game.RoundEnumerator.Current;
            }
        }

        public IEnumerable<Period> GetPeriods()
        {
            while (this.game.RoundEnumerator.Current.NextPeriod(this.game.Remaining ?? this.game.PeriodLength, this.game.TeamEnumerator.Current.PlayerEnumerator.Current))
            {
                yield return this.game.RoundEnumerator.Current.CurrentPeriod;
            }
        }

        public bool NextWord(DateTimeOffset timestamp, Word? previousWord = null)
        {
            var period = this.game.RoundEnumerator.Current.CurrentPeriod;

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
                this.game.TeamEnumerator.Current.PlayerEnumerator.MoveNext();
                this.game.TeamEnumerator.MoveNext();

                if (previousWord is null) this.game.RoundEnumerator.Current.Words.MovePrevious();
                return false;
            }

            if (!this.game.RoundEnumerator.Current.Words.MoveNext())
            {
                period.FinishedAt = timestamp;
                this.game.Remaining = period.StartedAt + period.Length - timestamp;
                return false;
            }

            return true;
        }
    }
}