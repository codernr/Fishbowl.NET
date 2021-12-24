using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Services;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    public record StartStateManagerTransitionAction(Type NextState);

    public record StateManagerTransitionEndAction(Type CurrentState);

    public class StateManagerEffects
    {
        private readonly IStateManagerService stateManagerService;

        public StateManagerEffects(IStateManagerService stateManagerService) =>
            this.stateManagerService = stateManagerService;

        [EffectMethod]
        public async Task OnStartStateManager(StartStateManagerTransitionAction action, IDispatcher dispatcher)
        {
            await this.stateManagerService.SetState(action.NextState);

            dispatcher.Dispatch(new StateManagerTransitionEndAction(action.NextState));
        }
    }
}