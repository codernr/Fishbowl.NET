using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record GameSetupState(string? Info = null, bool IsLoading = false);

    public record SubmitGameSetupAction(GameSetupViewModel GameSetup);

    public static class GameSetupReducers
    {
        [ReducerMethod(typeof(SubmitGameSetupAction))]
        public static GameSetupState OnSubmitGameSetup(GameSetupState state) => state with { IsLoading = true };
    }
}