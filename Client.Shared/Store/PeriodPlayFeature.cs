using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodPlayState
    {
        public PeriodRunningViewModel Period { get; init; } = default!;
        public bool Expired { get; init; } = false;
        public int ScoreCount { get; init; } = 0;
        public bool ShowRevoke { get; init; } = false;
        public WordViewModel? Word { get; init; } = null;
    };

    public record SetPeriodPlayPeriodAction(PeriodRunningViewModel Period);

    public record SetPeriodPlayScoreCountAction(int ScoreCount);

    public record SetPeriodPlayWordAction(WordViewModel Word);

    public record AddScoreAction(WordViewModel Word);

    public record RevokeLastScoreAction();

    public record FinishPeriodAction();

    public static class PeriodPlayReducers
    {
        [ReducerMethod]
        public static PeriodPlayState OnSetPeriod(PeriodPlayState state, SetPeriodPlayPeriodAction action) =>
            state with { Period = action.Period };

        [ReducerMethod]
        public static PeriodPlayState OnTimerExpired(PeriodPlayState state, TimerExpiredAction action) =>
            state with { Expired = true };

        [ReducerMethod]
        public static PeriodPlayState OnSetScoreCount(PeriodPlayState state, SetPeriodPlayScoreCountAction action) =>
            state with { ScoreCount = action.ScoreCount, ShowRevoke = action.ScoreCount > state.ScoreCount };

        [ReducerMethod]
        public static PeriodPlayState OnSetWord(PeriodPlayState state, SetPeriodPlayWordAction action) =>
            state with { Word = action.Word };
    }
}