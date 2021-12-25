using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodSetupState
    {
        public PeriodSetupViewModel Period { get; init; } = default!;
    };

    public record SetPeriodSetupAction(PeriodSetupViewModel Period);

    public record StartPeriodAction();

    public static class PeriodSetupReducers
    {
        [ReducerMethod]
        public static PeriodSetupState OnSetPeriodSetup(PeriodSetupState state, SetPeriodSetupAction action) =>
            state with { Period = action.Period };
    }
}