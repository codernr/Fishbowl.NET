using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodPlayState(
        PeriodRunningViewModel? Period = null,
        bool Expired = false,
        int ScoreCount = 0,
        bool ShowRevoke = false,
        WordViewModel? Word = null);

    public record SetPeriodPlayPeriodAction(PeriodRunningViewModel Period);

    public record SetPeriodPlayExpiredAction(bool Expired);

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
        public static PeriodPlayState OnSetExpired(PeriodPlayState state, SetPeriodPlayExpiredAction action) =>
            state with { Expired = action.Expired };

        [ReducerMethod]
        public static PeriodPlayState OnSetScoreCount(PeriodPlayState state, SetPeriodPlayScoreCountAction action) =>
            state with { ScoreCount = action.ScoreCount, ShowRevoke = action.ScoreCount > state.ScoreCount };

        [ReducerMethod]
        public static PeriodPlayState OnSetWord(PeriodPlayState state, SetPeriodPlayWordAction action) =>
            state with { Word = action.Word };
    }
}