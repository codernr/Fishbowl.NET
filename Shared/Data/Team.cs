using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Team
    {
        public string? Name { get; set; }
        
        public CircularEnumerator<Player> PlayerEnumerator { get; private set; }

        public int Id { get; private set; }

        public IList<Player> Players { get; private set; }

        public Team(int id, IList<Player> players) =>
            (this.Id, this.Players, this.PlayerEnumerator) =
            (id, players, new CircularEnumerator<Player>(players));
    }
}