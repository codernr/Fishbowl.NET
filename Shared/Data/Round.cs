using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        public string Type { get; init; }

        public IRewindEnumerator<Word> Words { get; init; }

        public Period CurrentPeriod
        { 
            get => this.periods[this.periods.Count - 1];
        }

        private readonly List<Period> periods = new List<Period>();

        public Round(string type, IRewindEnumerator<Word> words) =>
            (this.Type, this.Words) = (type, words);

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