using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Services;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    public record StateManagerTransitionAction(Type NextState, object? InterceptorAction = null, TimeSpan Delay = default);

    public record StateManagerTransitionEndAction(Type CurrentState);

    public class StateManagerEffects
    {
        private readonly IStateManagerService stateManagerService;

        public StateManagerEffects(IStateManagerService stateManagerService) =>
            this.stateManagerService = stateManagerService;

        [EffectMethod]
        public Task OnStateManagerTransition(StateManagerTransitionAction action, IDispatcher dispatcher) =>
            this.stateManagerService.SetState(
                action.NextState,
                action.InterceptorAction is null ? null : () => dispatcher.Dispatch(action.InterceptorAction),
                action.Delay);
    }
}