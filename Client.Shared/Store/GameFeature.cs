using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameState
    {
        public string? Info { get; init; } = null;
        public bool IsLoading { get; init; } = false;
        public GameSummaryViewModel Summary { get; init; } = default!;
    }

    public record SubmitGameSetupAction(GameSetupViewModel GameSetup);

    public record SetGameSummaryAction(GameSummaryViewModel Game);

    public static class GameSetupReducers
    {
        [ReducerMethod(typeof(SubmitGameSetupAction))]
        public static GameState OnSubmitGameSetup(GameState state) => state with { IsLoading = true };

        [ReducerMethod]
        public static GameState OnSetGameFinished(GameState state, SetGameSummaryAction action) =>
            state with { Summary = action.Game };
    }
}