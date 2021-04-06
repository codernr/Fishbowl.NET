using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Period
    {
        public double LengthInSeconds
        {
            get => this.length.TotalSeconds;
            init => this.length = TimeSpan.FromSeconds(value);
        }
        
        public Player Player { get; init; } = default!;
        
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<Score> Scores { get; init; } = new List<Score>();

        private TimeSpan length;

        public Period() {}

        public Period(TimeSpan length, Player player) =>
            (this.length, this.Player) = (length, player);

        public TimeSpan Length() => this.length;
    }
}