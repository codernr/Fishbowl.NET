using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record AppBarState(string Title = "");

    public record SetAppBarTitleAction(string Title);

    public static class AppBarReducers
    {
        [ReducerMethod]
        public static AppBarState OnSetAppBarTitle(AppBarState state, SetAppBarTitleAction action) =>
            new(action.Title);
    }
}