using Fluxor;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record PeriodWatchState
    {
        public bool ShowTeamAlert { get; init; }
    }

    public record SetPeriodWatchShowTeamAlertAction(bool ShowTeamAlert);

    public static class PeriodWatchReducers
    {
        [ReducerMethod]
        public static PeriodWatchState OnSetShowTeamAlert(PeriodWatchState state, SetPeriodWatchShowTeamAlertAction action) =>
            state with { ShowTeamAlert = action.ShowTeamAlert };
    }
}