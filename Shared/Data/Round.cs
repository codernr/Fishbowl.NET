using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        public string Type { get; init; }

        public EnumeratorList<Word> Words { get; init; }

        public Period CurrentPeriod
        { 
            get => this.periods[this.periods.Count - 1];
        }

        private readonly List<Period> periods = new EnumeratorList<Period>();

        public Round(string type, IEnumerable<Word> words) =>
            (this.Type, this.Words) = (type, new EnumeratorList<Word>(words));

        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.Words.MoveNext())
            {
                this.periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}