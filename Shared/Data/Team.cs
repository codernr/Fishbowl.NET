using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Team
    {
        public int Id { get; init; }

        public IEnumerable<Player> Players { get; init; }

        public CircularEnumerator<Player> PlayerEnumerator { get; init; }

        public Team(int id, IList<Player> players) =>
            (this.Id, this.Players, this.PlayerEnumerator) =
            (id, players, new CircularEnumerator<Player>(players));
    }
}