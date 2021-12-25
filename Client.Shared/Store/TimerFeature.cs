using System;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TimerState
    {
        public TimeSpan Remaining { get; init; } = default;
    }

    public record SetTimerAction(TimeSpan Remaining);

    public static class TimerReducers
    {
        [ReducerMethod]
        public static TimerState OnSetTimer(TimerState state, SetTimerAction action) =>
            new() { Remaining = action.Remaining };
    }
}