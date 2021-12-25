using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    public record ScreenManagerInitializedAction(ScreenManager ScreenManager);

    public record ScreenManagerTransitionAction(Type NextState, object? InterceptorAction = null, TimeSpan Delay = default);

    public record ScreenManagerTransitionEndAction(Type CurrentState);

    public class ScreenManagerEffects
    {
        private ScreenManager stateManager = default!;

        [EffectMethod]
        public Task OnScreenManagerInitialized(ScreenManagerInitializedAction action, IDispatcher dispatcher)
        {
            this.stateManager = action.ScreenManager;
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnScreenManagerTransition(ScreenManagerTransitionAction action, IDispatcher dispatcher) =>
            this.stateManager.SetState(action.NextState, action.InterceptorAction, action.Delay);
    }
}