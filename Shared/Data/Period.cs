using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Period(TimeSpan Length, Player Player)
    {
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public ICollection<Score> Scores { get; } = new List<Score>();
    }
}