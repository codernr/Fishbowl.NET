using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record AppBarState
    {
        public string Title { get; init; } = string.Empty;
    }

    public record SetAppBarTitleAction(string Title);

    public static class AppBarReducers
    {
        [ReducerMethod]
        public static AppBarState OnSetAppBarTitle(AppBarState state, SetAppBarTitleAction action) =>
            state with { Title = action.Title };
    }
}