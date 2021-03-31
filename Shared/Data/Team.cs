using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Team(int Id, IList<Player> Players)
    {
        public CircularEnumerator<Player> PlayersEnumerator { get; } = new CircularEnumerator<Player>(Players);
    }
}