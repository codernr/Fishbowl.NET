using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    [FeatureState]
    public record PlayerSetupState(int WordCount = 1, string Title = "");

    public record SetPlayerSetupAction(int WordCount, string Title);

    public record SubmitPlayerSetupAction(string PlayerName, string[] Words);

    public class PlayerSetupReducers
    {
        [ReducerMethod]
        public static PlayerSetupState OnSetPlayerSetup(PlayerSetupState state, SetPlayerSetupAction action) =>
            new(action.WordCount, action.Title);
    }
}