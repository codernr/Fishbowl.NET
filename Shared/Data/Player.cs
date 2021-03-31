using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Player(Guid Id, string Name, IEnumerable<Word> Words);
}