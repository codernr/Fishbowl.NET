using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    [FeatureState]
    public record PlayerSetupState
    {
        public int WordCount { get; init; } = 1;
        public string Title { get; init; } = string.Empty;
    }

    public record SetPlayerSetupAction(int WordCount, string Title);

    public record SubmitPlayerSetupAction(string PlayerName, string[] Words);

    public class PlayerSetupReducers
    {
        [ReducerMethod]
        public static PlayerSetupState OnSetPlayerSetup(PlayerSetupState state, SetPlayerSetupAction action) =>
            new() { WordCount = action.WordCount, Title = action.Title };
    }
}