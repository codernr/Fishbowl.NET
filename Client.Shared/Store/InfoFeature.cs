using Fluxor;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record InfoState(Severity Severity = Severity.Info, string? Title = null, string? Message = null, bool Loading = false);

    public record SetInfoAction(Severity Severity = Severity.Info, string? Title = null, string? Message = null, bool Loading = false);

    public static class InfoReducers
    {
        [ReducerMethod]
        public static InfoState OnSetInfo(InfoState state, SetInfoAction action) =>
            new(action.Severity, action.Title, action.Message, action.Loading);
    }
}