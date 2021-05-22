using System;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Period
    {
        public Player Player { get; init; } = default!;

        public TimeSpan Length { get; private set; }
        
        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public List<Score> Scores { get; init; } = new List<Score>();

        public Period(TimeSpan length, Player player) =>
            (this.Length, this.Player) = (length, player);

        public Score? RevokeLastScore()
        {
            var lastIndex = this.Scores.Count - 1;

            if (lastIndex < 0) return null;

            var score = this.Scores[lastIndex];
            this.Scores.RemoveAt(lastIndex);

            return score;
        }
    }
}