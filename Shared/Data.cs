using System;
using System.Collections;
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

        public Period CreatePeriod(Player player, TimeSpan remaining)
        {
            var length = remaining > PeriodThreshold ? remaining : PeriodLength;
            var period = new Period(length, player);
            this.Periods.Add(period);
            return period;
        }
    }

    public record Team(int Id, IList<Player> Players)
    {
        public CircularEnumerator<Player> PlayersEnumerator { get; } = new CircularEnumerator<Player>(Players);
    }

    public record Game(Guid Id, IList<Team> Teams, IList<Round> Rounds)
    {
        public CircularEnumerator<Team> TeamsEnumerator { get; } = new CircularEnumerator<Team>(Teams);

        public IEnumerator<Round> RoundsEnumerator { get; } = Rounds.GetEnumerator();

        public TimeSpan Remaining { get; set; }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Data { get; }

        public EventArgs(T data) => this.Data = data;
    }

    public class CircularEnumerator<T> : IEnumerator<T>
    {
        private readonly IList<T> list;

        private int id = 0;

        public CircularEnumerator(IList<T> list) => this.list = list;

        public T Current => this.list[this.id];

        object IEnumerator.Current => this.list[this.id]!;

        public void Dispose() { }

        public bool MoveNext()
        {
            this.id = (this.id + 1) % this.list.Count;
            return true;
        }

        public void Reset()
        {
            this.id = 0;
        }
    }
}