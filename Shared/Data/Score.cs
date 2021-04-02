using System;

namespace Fishbowl.Net.Shared.Data
{
    public class Score
    {
        public Word Word { get; private set; }
        
        public DateTimeOffset Timestamp { get; private set; }

        public Score(Word word, DateTimeOffset timestamp) =>
            (this.Word, this.Timestamp) = (word, timestamp);
    }
}