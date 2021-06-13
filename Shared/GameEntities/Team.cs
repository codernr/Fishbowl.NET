using System.Collections.Generic;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Team
    {
        public string? Name { get; set; }
        
        public CircularEnumerator<Player> PlayerEnumerator { get; private set; }

        public int Id { get; private set; }

        public IList<Player> Players => this.PlayerEnumerator.List;

        public Team(int id, IList<Player> players) =>
            (this.Id, this.PlayerEnumerator) =
            (id, new CircularEnumerator<Player>(players));

        public Team(string? name, int id, IList<Player> players, int currentPlayerIndex) =>
            (this.Name, this.Id, this.PlayerEnumerator) =
            (name, id, new CircularEnumerator<Player>(currentPlayerIndex, players));
    }
}