using Fishbowl.Net.Client.Shared.Store;
using Fluxor;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record PeriodWatchState : PeriodState
    {
        public bool ShowTeamAlert { get; init; }
    }

    public record SetPeriodWatchShowTeamAlertAction(bool ShowTeamAlert);

    public static class PeriodWatchReducers
    {
        [ReducerMethod]
        public static PeriodWatchState OnSetPeriod(PeriodWatchState state, SetPeriodPeriodAction action) =>
            new() { Period = action.Period };

        [ReducerMethod]
        public static PeriodWatchState OnSetShowTeamAlert(PeriodWatchState state, SetPeriodWatchShowTeamAlertAction action) =>
            state with { ShowTeamAlert = action.ShowTeamAlert };
    }
}