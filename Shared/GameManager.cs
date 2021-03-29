using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared
{
    public class GameManager
    {
        public event Action<Period>? PeriodStarted;

        public event Action<Period>? PeriodFinished;

        public event Action<Round>? RoundFinished;

        public event Action<Game>? GameFinished;

        public event Action<Score>? ScoreAdded;

        private readonly Game game;

        private int playerId = 0;

        public GameManager(Guid id, IEnumerable<Player> players, IEnumerable<string> roundTypes, int teamCount)
        {
            var randomPlayersList = players.Randomize().ToList();

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
                .Select(type => new Round(type, new Stack<Word>(words.Randomize())))
                .ToList();

            this.game = new Game(id, teams, rounds);
        }

        public (Period period, Word firstWord) SetupPeriod()
        {
            var period = this.game.NextRound.CreatePeriod(this.game.Players[this.playerId]);

            var word = this.game.NextRound.WordList.Pop();

            return (period, word);
        }

        public void StartPeriod(DateTimeOffset startedAt)
        {
            var period = this.game.ActualRound.Periods.Last();

            period.StartedAt = startedAt;

            this.PeriodStarted?.Invoke(period);
        }

        public void FinishPeriod(DateTimeOffset finishedAt)
        {
            var period = this.game.ActualRound.Periods.Last();

            period.FinishedAt = finishedAt;

            this.playerId = ++this.playerId % this.game.Players.Count;

            this.PeriodFinished?.Invoke(period);
        }

        public Word? AddScore(Score score)
        {
            var actualRound = this.game.ActualRound;
            var actualPeriod = actualRound.Periods.Last();

            actualPeriod.Scores.Add(score);

            if (actualRound.WordList.Count == 0)
            {
                actualPeriod.FinishedAt = score.Timestamp;

                if (this.game.Rounds.All(round => round.WordList.Count == 0))
                {
                    this.GameFinished?.Invoke(this.game);
                }
                else
                {
                    this.RoundFinished?.Invoke(actualRound);
                }

                return null;
            }

            if (score.Timestamp > actualPeriod.StartedAt! + actualPeriod.Length)
            {
                this.FinishPeriod(score.Timestamp);
                return null;
            }

            this.ScoreAdded?.Invoke(score);
            return actualRound.WordList.Pop();
        }
    }
}