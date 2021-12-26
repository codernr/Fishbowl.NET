using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodState
    {
        public PeriodSetupViewModel Setup { get; init; } = default!;
        public PeriodRunningViewModel Running { get; init; } = default!;
        public bool Expired { get; init; } = false;
        public int ScoreCount { get; init; } = 0;
        public bool ShowRevoke { get; init; } = false;
        public WordViewModel? Word { get; init; } = null;
        public PeriodSummaryViewModel Summary { get; init; } = default!;
    }

    public record StartPeriodAction();

    public record SetPeriodScoreCountAction(int ScoreCount);

    public record SetPeriodWordAction(WordViewModel Word);

    public record RevokeLastScoreAction();

    public record FinishPeriodAction();

    public static class PeriodReducers
    {
        [ReducerMethod]
        public static PeriodState OnReceivePeriodSetup(PeriodState state, ReceivePeriodSetupAction action) =>
            new() { Setup = action with { } as PeriodSetupViewModel };

        [ReducerMethod]
        public static PeriodState OnReceivePeriodStarted(PeriodState state, ReceivePeriodStartedAction action) =>
            state with { Running = action with { } as PeriodRunningViewModel };

        [ReducerMethod]
        public static PeriodState OnTimerExpired(PeriodState state, TimerExpiredAction action) =>
            state with { Expired = true };

        [ReducerMethod]
        public static PeriodState OnSetScoreCount(PeriodState state, SetPeriodScoreCountAction action) =>
            state with { ScoreCount = action.ScoreCount, ShowRevoke = action.ScoreCount > state.ScoreCount };

        [ReducerMethod]
        public static PeriodState OnSetWord(PeriodState state, SetPeriodWordAction action) =>
            state with { Word = action.Word };

        [ReducerMethod]
        public static PeriodState OnReceivePeriodFinished(PeriodState state, ReceivePeriodFinishedAction action) =>
            new() { Summary = action with { } as PeriodSummaryViewModel };
    }
}