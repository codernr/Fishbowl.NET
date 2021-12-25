using Fluxor;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record InfoState
    {
        public Severity Severity { get; init; } = Severity.Info;
        public string? Title { get; init; } = null;
        public string? Message { get; init; } = null;
        public bool Loading { get; init; } = false;
    }

    public record SetInfoAction(Severity Severity = Severity.Info, string? Title = null, string? Message = null, bool Loading = false);

    public static class InfoReducers
    {
        [ReducerMethod]
        public static InfoState OnSetInfo(InfoState state, SetInfoAction action) => state with
        {
            Severity = action.Severity,
            Title = action.Title,
            Message = action.Message,
            Loading = action.Loading
        };
    }
}