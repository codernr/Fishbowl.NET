using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Period
    {
        public Period(TimeSpan length, Player player) =>
            (this.Length, this.Player) = (length, player);
            
        public TimeSpan Length { get; init; }
        
        public Player Player { get; init; }
        
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public ICollection<Score> Scores { get; } = new List<Score>();
    }
}