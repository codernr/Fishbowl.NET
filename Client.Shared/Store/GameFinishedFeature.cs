using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameFinishedState(GameSummaryViewModel? Game = null);

    public record GameFinishedAction(GameSummaryViewModel Game);

    public static class GameFinishedReducers
    {
        [ReducerMethod]
        public static GameFinishedState OnGameFinished(GameFinishedState state, GameFinishedAction action) =>
            new(action.Game);
    }
}