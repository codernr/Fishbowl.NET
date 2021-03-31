using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Game(Guid Id, IList<Team> Teams, IList<Round> Rounds)
    {
        public CircularEnumerator<Team> TeamsEnumerator { get; } = new CircularEnumerator<Team>(Teams);

        public IEnumerator<Round> RoundsEnumerator { get; } = Rounds.GetEnumerator();

        public TimeSpan Remaining { get; set; }
    }
}