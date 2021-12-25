using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameFinishedState
    {
        public GameSummaryViewModel Game { get; init; } = default!;
    }

    public record SetGameFinishedAction(GameSummaryViewModel Game);

    public static class GameFinishedReducers
    {
        [ReducerMethod]
        public static GameFinishedState OnSetGameFinished(GameFinishedState state, SetGameFinishedAction action) =>
            state with { Game = action.Game };
    }
}