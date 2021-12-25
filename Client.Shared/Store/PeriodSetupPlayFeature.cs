using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodSetupPlayState
    {
        public PeriodSetupViewModel Period { get; init; } = default!;
    };

    public record SetPeriodSetupPlayAction(PeriodSetupViewModel Period);

    public record StartPeriodAction();

    public static class PeriodSetupPlayReducers
    {
        [ReducerMethod]
        public static PeriodSetupPlayState OnSetPeriodSetupPlay(PeriodSetupPlayState state, SetPeriodSetupPlayAction action) =>
            new() { Period = action.Period };
    }
}