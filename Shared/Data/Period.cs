using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Period
    {    
        public TimeSpan Length { get; init; } = default!;
        
        public Player Player { get; init; } = default!;
        
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<Score> Scores { get; init; } = new List<Score>();

        public Period() {}

        public Period(TimeSpan length, Player player) =>
            (this.Length, this.Player) = (length, player);
    }
}