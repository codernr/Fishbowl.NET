using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        private readonly IReturnEnumerator<Word>? wordEnumerator;

        public string Type { get; init; } = default!;

        public List<Period> Periods
        {
            get => this.periods;
            init => this.periods = value;
        }

        private List<Period> periods = new List<Period>();

        public Round() {}

        public Round(string type, IReturnEnumerator<Word> wordEnumerator) =>
            (this.Type, this.wordEnumerator) = (type, wordEnumerator);

        public IReturnEnumerator<Word> WordEnumerator() => this.wordEnumerator ?? throw new InvalidOperationException();

        public Period CurrentPeriod() => this.periods[this.periods.Count - 1];
        
        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.WordEnumerator().MoveNext())
            {
                this.periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}