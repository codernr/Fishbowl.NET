using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Period
    {    
        public TimeSpan Length { get; private set; }
        
        public Player Player { get; private set; }
        
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public ICollection<Score> Scores { get; } = new List<Score>();

        public Period(TimeSpan length, Player player) =>
            (this.Length, this.Player) = (length, player);
    }
}