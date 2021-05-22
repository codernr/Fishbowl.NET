using System;
using System.Collections.Generic;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        public IReturnEnumerator<Word> WordEnumerator { get; private set; }

        public string Type { get; private set; }

        public List<Period> Periods { get; } = new();

        public Period CurrentPeriod => this.Periods[this.Periods.Count - 1];

        public Round(string type, IReturnEnumerator<Word> wordEnumerator) =>
            (this.Type, this.WordEnumerator) = (type, wordEnumerator);

        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.WordEnumerator.MoveNext())
            {
                this.Periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}