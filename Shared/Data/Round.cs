using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record Round(string Type, Stack<Word> WordList)
    {
        private static readonly TimeSpan PeriodThreshold = TimeSpan.FromSeconds(5);

        private static readonly TimeSpan PeriodLength = TimeSpan.FromSeconds(60);

        public ICollection<Period> Periods { get; } = new List<Period>();

        public Period CreatePeriod(Player player, TimeSpan remaining)
        {
            var length = remaining > PeriodThreshold ? remaining : PeriodLength;
            var period = new Period(length, player);
            this.Periods.Add(period);
            return period;
        }
    }
}