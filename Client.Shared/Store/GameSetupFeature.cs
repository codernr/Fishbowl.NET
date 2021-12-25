using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameSetupState
    {
        public string? Info { get; init; } = null;
        public bool IsLoading { get; init; } = false;
    }

    public record SubmitGameSetupAction(GameSetupViewModel GameSetup);

    public static class GameSetupReducers
    {
        [ReducerMethod(typeof(SubmitGameSetupAction))]
        public static GameSetupState OnSubmitGameSetup(GameSetupState state) => state with { IsLoading = true };
    }
}