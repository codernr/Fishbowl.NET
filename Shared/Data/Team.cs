using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Team
    {
        private readonly CircularEnumerator<Player> playerEnumerator;

        public int Id { get; private set; }

        public IList<Player> Players { get; private set; }

        public Team(int id, IList<Player> players) =>
            (this.Id, this.Players, this.playerEnumerator) =
            (id, players, new CircularEnumerator<Player>(players));

        public IEnumerator<Player> PlayerEnumerator() => this.playerEnumerator;
    }
}