using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record PeriodStateCollection
    {
        public int CurrentId { get; init; } = -1;

        public ImmutableList<PeriodState> PeriodStates = ImmutableList<PeriodState>.Empty;

        [JsonIgnore]
        public PeriodState Current => this.PeriodStates[this.CurrentId];

        [JsonIgnore]
        public PeriodState Last => this.PeriodStates[this.PeriodStates.Count - 1];
    }

    public record PeriodState
    {
        public PeriodSetupViewModel Setup { get; init; } = default!;
        public PeriodRunningViewModel Running { get; init; } = default!;
        public TimeSpan Remaining { get; init; } = default;
        public bool ShowRevoke { get; init; } = false;
        public WordViewModel? Word { get; init; } = null;
        public PeriodSummaryViewModel Summary { get; init; } = default!;
        public ImmutableList<ScoreViewModel> Scores { get; init; } = ImmutableList<ScoreViewModel>.Empty;
    }

    public record StartPeriodAction();

    public record IncrementPeriodCurrentIdAction();

    public record RevokeLastScoreAction();

    public record FinishPeriodAction();

    public static class PeriodReducers
    {
        [ReducerMethod]
        public static PeriodStateCollection OnReceivePeriodSetup(PeriodStateCollection state, ReceivePeriodSetupAction action) =>
            state with { PeriodStates = state.PeriodStates.Add(new() { Setup = action.Setup, Remaining = action.Setup.Length }) };

        [ReducerMethod(typeof(IncrementPeriodCurrentIdAction))]
        public static PeriodStateCollection OnIncrementPeriodCurrentIdAction(PeriodStateCollection state) =>
            state with { CurrentId = state.CurrentId + 1 };

        [ReducerMethod]
        public static PeriodStateCollection OnReceivePeriodStarted(PeriodStateCollection state, ReceivePeriodStartedAction action) =>
            state.Update(state.Last with { Running = action.Period });

        [ReducerMethod]
        public static PeriodStateCollection ReceiveTimerUpdate(PeriodStateCollection state, ReceiveTimerUpdateAction action) =>
           state.Update(state.Last with { Remaining = action.Remaining });

        [ReducerMethod]
        public static PeriodStateCollection OnReceiveScoreAdded(PeriodStateCollection state, ReceiveScoreAddedAction action) =>
            state.Update(state.Last with { Scores = state.Last.Scores.Add(action.Score), ShowRevoke = true });

        [ReducerMethod]
        public static PeriodStateCollection OnReceiveLastScoreRevoked(PeriodStateCollection state, ReceiveLastScoreRevokedAction action) =>
            state.Update(state.Last with
            {
                Scores = state.Last.Scores.Remove(action.Score),
                ShowRevoke = false,
                Word = action.Score.Word
            });

        [ReducerMethod]
        public static PeriodStateCollection OnReceiveWordSetup(PeriodStateCollection state, ReceiveWordSetupAction action) =>
            state.Update(state.Last with { Word = action.Word });

        [ReducerMethod]
        public static PeriodStateCollection OnReceivePeriodFinished(PeriodStateCollection state, ReceivePeriodFinishedAction action) =>
            state.Update(state.Last with { Summary = action.Summary });

        public static PeriodStateCollection Update(this PeriodStateCollection state, PeriodState newState) =>
            state with { PeriodStates = state.PeriodStates.Replace(state.Last, newState) };
    }
}