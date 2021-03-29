using System;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.Data
{
    public record Word(Guid Id, string Value);

    public record Player(Guid Id, string Name, IEnumerable<Word> Words);

    public record Score(Word word, DateTimeOffset Timestamp);

    public record Period(TimeSpan Length, Player Player)
    {
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public ICollection<Score> Scores { get; } = new List<Score>();
    }

    public record Round(string Type, Stack<Word> WordList)
    {
        private static readonly TimeSpan PeriodThreshold = TimeSpan.FromSeconds(5);

        private static readonly TimeSpan PeriodLength = TimeSpan.FromSeconds(60);

        public ICollection<Period> Periods { get; } = new List<Period>();

        public Period CreatePeriod(Player player)
        {
            var period = new Period(this.GetNextPeriodLength(), player);
            this.Periods.Add(period);
            return period;
        }

        private TimeSpan GetNextPeriodLength()
        {
            if (this.Periods.Count == 0)
            {
                return PeriodLength;
            }
            
            var last = this.Periods.Last();

            if (last.StartedAt is null || last.FinishedAt is null)
            {
                throw new InvalidOperationException("Can't invoke when period is active");
            }

            var end = last.StartedAt.Value + last.Length - PeriodThreshold;

            if (end <= last.FinishedAt)
            {
                return PeriodLength;
            }

            return end - last.FinishedAt.Value;
        }
    }

    public record Team(int Id, IList<Player> Players);

    public record Game(Guid Id, IList<Team> Teams, IList<Round> Rounds)
    {
        public IList<Player> Players { get; } = Teams.SelectMany(team => team.Players).ToList();

        public int WordCount { get; } = Teams
            .SelectMany(team => team.Players)
            .SelectMany(player => player.Words)
            .Count();

        public Round ActualRound
        {
            get => this.Rounds.Last(round => round.WordList.Count < this.WordCount);
        }

        public Round NextRound
        {
            get => this.Rounds.First(round => round.WordList.Count > 0);
        }
    }
}