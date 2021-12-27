using System.Collections.Generic;
using System.Linq;
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
        public bool ShowRevoke { get; init; } = false;
        public WordViewModel? Word { get; init; } = null;
        public PeriodSummaryViewModel Summary { get; init; } = default!;
        public List<ScoreViewModel> Scores { get; init; } = default!;
    }

    public record StartPeriodAction();

    public record RevokeLastScoreAction();

    public record FinishPeriodAction();

    public static class PeriodReducers
    {
        [ReducerMethod]
        public static PeriodState OnReceivePeriodSetup(PeriodState state, ReceivePeriodSetupAction action) =>
            state with
            { 
                Setup = action.Setup,
                ShowRevoke = false,
                Expired = false,
                Word = null
            };

        [ReducerMethod]
        public static PeriodState OnReceivePeriodStarted(PeriodState state, ReceivePeriodStartedAction action) =>
            state with { Running = action.Period, Scores = new() };

        [ReducerMethod]
        public static PeriodState OnTimerExpired(PeriodState state, TimerExpiredAction action) =>
            state with { Expired = true };

        [ReducerMethod]
        public static PeriodState OnReceiveScoreAdded(PeriodState state, ReceiveScoreAddedAction action)
        {
            state.Scores.Add(action.Score);
            return state with { Scores = new(state.Scores), ShowRevoke = true };
        }

        [ReducerMethod]
        public static PeriodState OnReceiveLastScoreRevoked(PeriodState state, ReceiveLastScoreRevokedAction action) =>
            state with
            {
                Scores = state.Scores.Where(score => score.Word.Id != action.Score.Word.Id ).ToList(),
                ShowRevoke = false,
                Word = action.Score.Word
            };

        [ReducerMethod]
        public static PeriodState OnReceiveWordSetup(PeriodState state, ReceiveWordSetupAction action) =>
            state with { Word = action.Word };

        [ReducerMethod]
        public static PeriodState OnReceivePeriodFinished(PeriodState state, ReceivePeriodFinishedAction action) =>
            state with { Summary = action.Summary };
    }
}