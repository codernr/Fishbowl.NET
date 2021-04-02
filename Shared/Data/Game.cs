using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Game
    { 
        public Guid Id { get; init; }

        public IEnumerable<Team> Teams { get; init; }
        
        public CircularEnumerator<Team> TeamEnumerator { get; init; }

        public IEnumerable<Round> Rounds { get; init; }
        
        public IEnumerator<Round> RoundEnumerator { get; init; }

        public TimeSpan? Remaining { get; set; }

        public TimeSpan PeriodLength { get; init;} = TimeSpan.FromSeconds(60);

        public static readonly TimeSpan PeriodThreshold = TimeSpan.FromSeconds(5);

        public Game(Guid id, IList<Team> teams, IList<Round> rounds) =>
            (this.Id, this.Teams, this.TeamEnumerator, this.Rounds, this.RoundEnumerator) =
            (id, teams, new CircularEnumerator<Team>(teams), rounds, rounds.GetEnumerator());
    }
}