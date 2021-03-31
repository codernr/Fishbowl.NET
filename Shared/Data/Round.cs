using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        public string Type { get; init; }

        public Stack<Word> WordList { get; init; }

        public Period CurrentPeriod
        { 
            get => this.periods[this.periods.Count - 1];
        }

        private readonly List<Period> periods = new EnumeratorList<Period>();

        public Round(string type, IEnumerable<Word> wordList) =>
            (this.Type, this.WordList) = (type, new Stack<Word>(wordList));

        public bool NextPeriod(TimeSpan length, Player player)
        {
            if (this.WordList.Count > 0)
            {
                this.periods.Add(new Period(length, player));
                return true;
            }

            return false;
        }
    }
}