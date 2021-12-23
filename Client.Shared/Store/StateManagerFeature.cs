using System;
using Fluxor;

namespace Fishbowl.Net.Shared.Store
{
    [FeatureState]
    public record StateManagerState(bool IsTransitioning = false, Type? CurrentState = null);

    public record StateManagerStartTransitionAction(Type NextState);

    public record StateManagerTransitionEndAction();

    public static class StateManagerReducers
    {
        [ReducerMethod]
        public static StateManagerState OnStartTransition(StateManagerState state, StateManagerStartTransitionAction action) =>
            new(true, action.NextState);

        [ReducerMethod(typeof(StateManagerTransitionEndAction))]
        public static StateManagerState OnTransitionEnd(StateManagerState state) => state with { IsTransitioning = false };
    }
}