using System.Collections.Generic;

namespace Fishbowl.Net.Shared.GameEntities
{
    public record Player(string Username, IEnumerable<Word> Words);
}