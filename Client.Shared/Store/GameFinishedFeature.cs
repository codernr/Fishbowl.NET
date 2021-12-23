using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameFinishedState(GameSummaryViewModel? Game = null);

    public record SetGameFinishedAction(GameSummaryViewModel Game);

    public static class GameFinishedReducers
    {
        [ReducerMethod]
        public static GameFinishedState OnSetGameFinished(GameFinishedState state, SetGameFinishedAction action) =>
            new(action.Game);
    }
}