using System;

namespace Fishbowl.Net.Shared.Data
{
    public class Score
    {
        public Word Word { get; init; }
        
        public DateTimeOffset Timestamp { get; init; }

        public Score(Word word, DateTimeOffset timestamp) =>
            (this.Word, this.Timestamp) = (word, timestamp);
    }
}