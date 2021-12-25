using Fluxor;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record PlayerWordsState
    {
        public string Message { get; init; } = string.Empty;
        public int WordCount { get; init; } = 2;
    }

    public record SetPlayerWordsAction(string Message, int WordCount);

    public record SubmitPlayerWordsAction(string[] Words);

    public static class PlayerWordsReducers
    {
        [ReducerMethod]
        public static PlayerWordsState OnSetPlayerWords(PlayerWordsState state, SetPlayerWordsAction action) =>
            state with { Message = action.Message, WordCount = action.WordCount };
    }
}