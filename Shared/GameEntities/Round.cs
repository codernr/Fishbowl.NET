using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class Round
    {
        [JsonInclude]
        public IRewindList<Word> Words { get; private set; } = default!;

        [JsonInclude]
        public string Type { get; private set; } = default!;

        [JsonInclude]
        public List<Period> Periods { get; private set; } = new();

        [JsonIgnore]
        public Period CurrentPeriod => this.Periods[this.Periods.Count - 1];

        public Round() {}

        public Round(string type, IRewindList<Word> words) =>
            (this.Type, this.Words) = (type, words);

        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.Words.NextItems.Count > 0)
            {
                this.Periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}