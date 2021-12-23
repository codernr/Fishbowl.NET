using System;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TimerState(TimeSpan Remaining = default);

    public record SetTimerAction(TimeSpan Remaining);

    public static class TimerReducers
    {
        [ReducerMethod]
        public static TimerState OnSetTimer(TimerState state, SetTimerAction action) =>
            new(action.Remaining);
    }
}