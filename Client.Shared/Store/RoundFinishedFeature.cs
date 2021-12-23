using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record RoundFinishedState(RoundSummaryViewModel? Round);

    public record SetRoundFinishedAction(RoundSummaryViewModel Round);

    public static class RoundFinishedReducers
    {
        [ReducerMethod]
        public static RoundFinishedState OnSetRoundFinished(RoundFinishedState state, SetRoundFinishedAction action) =>
            new(action.Round);
    }
}