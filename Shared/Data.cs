using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Word(Guid Id, string Value);

    public record Player(Guid Id, string Name, IEnumerable<Word> Words);

    public record Score(Word word, DateTimeOffset timestamp);

    public record Period(int Length, Player player)
    {
        public DateTimeOffset StartedAt { get; set; }

        public DateTimeOffset FinishedAt { get; set; }

        public ICollection<Score> Scores { get; } = new List<Score>();
    }

    public record Round(string Type)
    {
        public ICollection<Period> Periods { get; } = new List<Period>();
    }

    public record Team(int Id, IList<Player> Players);

    public record Game(Guid Id, IList<Team> Teams, IList<Round> Rounds);
}