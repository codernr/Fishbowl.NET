using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodFinishedState(PeriodSummaryViewModel? Period = null);

    public record SetPeriodFinishedAction(PeriodSummaryViewModel Period);

    public static class PeriodFinishedReducers
    {
        [ReducerMethod]
        public static PeriodFinishedState OnSetPeriodFinished(PeriodFinishedState state, SetPeriodFinishedAction action) =>
            new(action.Period);
    }
}