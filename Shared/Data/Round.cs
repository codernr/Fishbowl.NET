using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Round
    {
        private static readonly TimeSpan PeriodThreshold = TimeSpan.FromSeconds(5);

        private static readonly TimeSpan PeriodLength = TimeSpan.FromSeconds(60);

        public string Type { get; init; }

        public ICollection<Period> Periods { get; } = new List<Period>();

        public Stack<Word> WordList { get; init; }

        public Round(string type, IEnumerable<Word> wordList) =>
            (this.Type, this.WordList) = (type, new Stack<Word>(wordList));

        public Period CreatePeriod(Player player, TimeSpan remaining)
        {
            var length = remaining > PeriodThreshold ? remaining : PeriodLength;
            var period = new Period(length, player);
            this.Periods.Add(period);
            return period;
        }
    }
}