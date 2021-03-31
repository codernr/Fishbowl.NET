using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Game
    {
        public Guid Id { get; init; }
        
        public CircularEnumeratorList<Team> Teams { get; init; }
        
        public EnumeratorList<Round> Rounds { get; init; }

        public TimeSpan Remaining { get; set; }

        public Game(Guid id, IEnumerable<Team> teams, IEnumerable<Round> rounds) =>
            (this.Id, this.Teams, this.Rounds) =
            (id, new CircularEnumeratorList<Team>(teams), new EnumeratorList<Round>(rounds));
    }
}