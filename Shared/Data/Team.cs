using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Team
    {
        public int Id { get; init; }

        public CircularEnumeratorList<Player> Players;

        public Team(int id, IEnumerable<Player> players) =>
            (this.Id, this.Players) = (id, new CircularEnumeratorList<Player>(players));
    }
}