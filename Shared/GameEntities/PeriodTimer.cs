using System;
using System.Timers;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class PeriodTimer : IDisposable
    {
        private readonly Action<TimeSpan>? timerUpdate;

        private readonly Timer timer;

        private readonly DateTimeOffset start;

        private readonly TimeSpan length;

        public PeriodTimer(
            Action<TimeSpan>? timerUpdate,
            DateTimeOffset start,
            TimeSpan length)
        {
            this.timerUpdate = timerUpdate;
            this.start = start;
            this.length = length;
            this.timer = new() { Interval = 250, AutoReset = true };
            this.timer.Elapsed += this.Update;

            this.timer.Start();
        }

        private void Update(object? sender, ElapsedEventArgs e) =>
            this.timerUpdate?.Invoke(this.start + this.length - DateTimeOffset.UtcNow);

        public void Dispose()
        {
            this.timer.Stop();
            this.timer.Elapsed -= this.Update;
            this.timer.Dispose();
        }
    }
}