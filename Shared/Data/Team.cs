using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Team
    {
        public int Id { get; init; }

        public CircularEnumerator<Player> Players;

        public Team(int id, IEnumerable<Player> players) =>
            (this.Id, this.Players) = (id, new CircularEnumerator<Player>(players));
    }
}