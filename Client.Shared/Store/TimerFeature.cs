using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TimerState
    {
        public TimeSpan Remaining { get; init; } = default;
    }

    public record SetTimerAction(TimeSpan Remaining);

    public record TimerExpiredAction();

    public static class TimerReducers
    {
        [ReducerMethod]
        public static TimerState OnSetTimer(TimerState state, SetTimerAction action) =>
            new() { Remaining = action.Remaining };
    }

    public class TimerEffects
    {
        private bool isRunning = false;

        [EffectMethod]
        public async Task OnSetPeriodPlayPeriod(SetPeriodPlayPeriodAction action, IDispatcher dispatcher)
        {
            this.isRunning = true;

            var remaining = GetRemaining(action.Period);
            dispatcher.Dispatch(new SetTimerAction(remaining));

            TimeSpan previous;

            while (this.isRunning)
            {
                previous = remaining;
                await Task.Delay(500);
                remaining = GetRemaining(action.Period);
                dispatcher.Dispatch(new SetTimerAction(remaining));

                if (remaining < TimeSpan.Zero && previous > TimeSpan.Zero)
                {
                    dispatcher.Dispatch(new TimerExpiredAction());
                }
            }
        }

        [EffectMethod(typeof(SetPeriodFinishedAction))]
        public Task OnFinishPeriod(IDispatcher dispatcher)
        {
            this.isRunning = false;
            return Task.CompletedTask;
        }

        private static TimeSpan GetRemaining(PeriodRunningViewModel period) =>
            period.StartedAt + period.Length - DateTimeOffset.UtcNow;
    }
}