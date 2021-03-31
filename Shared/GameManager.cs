using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared
{
    public class GameManager
    {
        private readonly Game game;

        public GameManager(Guid id, IEnumerable<Player> players, IEnumerable<string> roundTypes, int teamCount, bool randomize = true)
        {
            var randomPlayersList = (randomize ? players.Randomize() : players).ToList();

            if (randomPlayersList.Count < teamCount * 2)
            {
                throw new ArgumentException("Player count should be at least (team count) * 2.");
            }

            var words = randomPlayersList
                .SelectMany(player => player.Words)
                .ToList();

            var teams = randomPlayersList
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()))
                .ToList();

            var rounds = roundTypes
                .Select(type => new Round(type, new Stack<Word>(randomize ? words.Randomize() : words)))
                .ToList();

            this.game = new Game(id, teams, rounds);
        }

        public async IAsyncEnumerable<Round> GetRounds()
        {
            while(this.game.RoundsEnumerator.MoveNext())
            {
                yield return this.game.RoundsEnumerator.Current;
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<Period> GetPeriods()
        {
            while (this.game.RoundsEnumerator.Current.WordList.Count > 0)
            {
                yield return this.game.RoundsEnumerator.Current.CreatePeriod(
                    this.game.TeamsEnumerator.Current.PlayersEnumerator.Current, this.game.Remaining);
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<Word> GetWords(IAsyncEnumerable<(Word?, DateTimeOffset)> submissions)
        {
            var period = this.game.RoundsEnumerator.Current.Periods.Last();

            await foreach (var (word, timestamp) in submissions)
            {
                // submission indicating start of period
                if (period.StartedAt is null)
                {
                    period.StartedAt = timestamp;
                    yield return this.game.RoundsEnumerator.Current.WordList.Pop();
                    continue;
                }

                // submission indicating pass last guess
                if (word is null)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = TimeSpan.Zero;
                    this.game.TeamsEnumerator.Current.PlayersEnumerator.MoveNext();
                    this.game.TeamsEnumerator.MoveNext();
                    yield break;
                }

                period.Scores.Add(new Score(word, timestamp));

                // last guess over time
                if (timestamp >= period.StartedAt + period.Length)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = TimeSpan.Zero;
                    this.game.TeamsEnumerator.Current.PlayersEnumerator.MoveNext();
                    this.game.TeamsEnumerator.MoveNext();
                    yield break;
                }

                // no more words, end round
                if (this.game.RoundsEnumerator.Current.WordList.Count == 0)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = period.StartedAt.Value + period.Length - timestamp;
                    yield break;
                }

                yield return this.game.RoundsEnumerator.Current.WordList.Pop();
            }
        }
    }
}