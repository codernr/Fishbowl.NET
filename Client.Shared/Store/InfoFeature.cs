using Fluxor;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record InfoState(Severity Severity = Severity.Info, string? Message = null, bool Loading = false);

    public record SetInfoStateAction(Severity Severity = Severity.Info, string? Message = null, bool Loading = false);

    public static class InfoReducers
    {
        [ReducerMethod]
        public static InfoState OnSetInfoState(InfoState state, SetInfoStateAction action) =>
            new(action.Severity, action.Message, action.Loading);
    }
}