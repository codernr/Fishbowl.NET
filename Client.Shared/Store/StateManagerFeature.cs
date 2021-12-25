using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Services;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    public record StateManagerInitializedAction(StateManager StateManager);

    public record StateManagerTransitionAction(Type NextState, object? InterceptorAction = null, TimeSpan Delay = default);

    public record StateManagerTransitionEndAction(Type CurrentState);

    public class StateManagerEffects
    {
        private StateManager stateManager = default!;

        [EffectMethod]
        public Task OnStateManagerInitialized(StateManagerInitializedAction action, IDispatcher dispatcher)
        {
            this.stateManager = action.StateManager;
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnStateManagerTransition(StateManagerTransitionAction action, IDispatcher dispatcher) =>
            this.stateManager.SetState(action.NextState, action.InterceptorAction, action.Delay);
    }
}