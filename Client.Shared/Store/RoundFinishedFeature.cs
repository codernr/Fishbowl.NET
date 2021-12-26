using System.Collections.Generic;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record RoundFinishedState
    {
        public string Type { get; init; } = string.Empty;

        public List<PeriodSummaryViewModel> Periods = default!;
    }

    public static class RoundFinishedReducers
    {
        [ReducerMethod]
        public static RoundFinishedState OnSetRoundFinished(RoundFinishedState state, ReceiveRoundFinishedAction action) =>
            state with { Type = action.Type, Periods = action.Periods };
    }
}