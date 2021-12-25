using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodFinishedState
    {
        public PeriodSummaryViewModel Period { get; init; } = default!;
    }

    public record SetPeriodFinishedAction(PeriodSummaryViewModel Period);

    public static class PeriodFinishedReducers
    {
        [ReducerMethod]
        public static PeriodFinishedState OnSetPeriodFinished(PeriodFinishedState state, SetPeriodFinishedAction action) =>
            new() { Period = action.Period };
    }
}