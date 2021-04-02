using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        private readonly IRewindEnumerator<Word> wordEnumerator;

        public string Type { get; init; }

        public IEnumerable<Period> Periods => this.periods;

        private readonly List<Period> periods = new List<Period>();

        public Round(string type, IRewindEnumerator<Word> wordEnumerator) =>
            (this.Type, this.wordEnumerator) = (type, wordEnumerator);

        public IRewindEnumerator<Word> WordEnumerator() => this.wordEnumerator;

        public Period CurrentPeriod() => this.periods[this.periods.Count - 1];
        
        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.wordEnumerator.MoveNext())
            {
                this.periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}