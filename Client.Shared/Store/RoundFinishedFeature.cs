using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record RoundFinishedState
    {
        public RoundSummaryViewModel Round { get; init; } = default!;
    }

    public record SetRoundFinishedAction(RoundSummaryViewModel Round);

    public static class RoundFinishedReducers
    {
        [ReducerMethod]
        public static RoundFinishedState OnSetRoundFinished(RoundFinishedState state, SetRoundFinishedAction action) =>
            new() { Round = action.Round };
    }
}