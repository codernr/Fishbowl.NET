using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TimerState
    {
        public TimeSpan Remaining { get; init; } = default;
    }

    public static class TimerReducers
    {
        [ReducerMethod]
        public static TimerState OnSetTimer(TimerState state, ReceiveTimerUpdateAction action) =>
            new() { Remaining = action.Remaining };
    }
}