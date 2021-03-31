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
                .SelectMany(player => player.Words);

            var teams = randomPlayersList
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()));

            var rounds = roundTypes
                .Select(type => new Round(type, randomize ? words.Randomize() : words));

            this.game = new Game(id, teams, rounds);
        }

        public async IAsyncEnumerable<Round> GetRounds()
        {
            while(this.game.Rounds.MoveNext())
            {
                yield return this.game.Rounds.Current;
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<Period> GetPeriods()
        {
            while (this.game.Rounds.Current.NextPeriod(this.game.Remaining ?? this.game.PeriodLength, this.game.Teams.Current.Players.Current))
            {
                yield return this.game.Rounds.Current.CurrentPeriod;
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<(Word word, Score? score)> GetWords(IAsyncEnumerable<(Word?, DateTimeOffset)> submissions)
        {
            var period = this.game.Rounds.Current.CurrentPeriod;

            await foreach (var (word, timestamp) in submissions)
            {
                // submission indicating start of period
                if (period.StartedAt is null)
                {
                    period.StartedAt = timestamp;
                    yield return (this.game.Rounds.Current.WordList.Pop(), null);
                    continue;
                }

                // submission indicating pass last guess
                if (word is null)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = null;
                    this.game.Teams.Current.Players.MoveNext();
                    this.game.Teams.MoveNext();
                    yield break;
                }

                var score = new Score(word, timestamp);
                period.Scores.Add(score);

                // last guess over time
                if (timestamp >= period.StartedAt + period.Length)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = null;
                    this.game.Teams.Current.Players.MoveNext();
                    this.game.Teams.MoveNext();
                    yield break;
                }

                // no more words, end round
                if (this.game.Rounds.Current.WordList.Count == 0)
                {
                    period.FinishedAt = timestamp;
                    this.game.Remaining = period.StartedAt.Value + period.Length - timestamp;
                    yield break;
                }

                yield return (this.game.Rounds.Current.WordList.Pop(), score);
            }
        }
    }
}