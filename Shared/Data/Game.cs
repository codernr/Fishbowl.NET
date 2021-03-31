using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Game
    { 
        public Guid Id { get; init; }
        
        public CircularEnumeratorList<Team> Teams { get; init; }
        
        public EnumeratorList<Round> Rounds { get; init; }

        public TimeSpan? Remaining { get; set; }

        public TimeSpan PeriodLength { get; init;} = TimeSpan.FromSeconds(60);

        public static readonly TimeSpan PeriodThreshold = TimeSpan.FromSeconds(5);

        public Game(Guid id, IEnumerable<Team> teams, IEnumerable<Round> rounds) =>
            (this.Id, this.Teams, this.Rounds) =
            (id, new CircularEnumeratorList<Team>(teams), new EnumeratorList<Round>(rounds));
    }
}