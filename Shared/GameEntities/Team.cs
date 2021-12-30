using System.Collections.Generic;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Team
    {
        [JsonInclude]
        public int Id { get; private set; }

        [JsonInclude]
        public string? Name { get; set; }

        [JsonInclude]
        public CircularList<Player> Players { get; private set; } = default!;

        public Team() {}

        public Team(int id, IList<Player> players) =>
            (this.Id, this.Players) = (id, new(players));
    }
}