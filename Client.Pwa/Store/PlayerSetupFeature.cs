using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    [FeatureState]
    public record PlayerSetupState
    {
        public int PlayerIndex = 0;
        public int WordCount { get; init; } = 1;
    }

    public record SetPlayerSetupAction(int PlayerIndex, int WordCount);

    public record SubmitPlayerSetupAction(string PlayerName, string[] Words);

    public class PlayerSetupReducers
    {
        [ReducerMethod]
        public static PlayerSetupState OnSetPlayerSetup(PlayerSetupState state, SetPlayerSetupAction action) =>
            new() { PlayerIndex = action.PlayerIndex, WordCount = action.WordCount };
    }
}