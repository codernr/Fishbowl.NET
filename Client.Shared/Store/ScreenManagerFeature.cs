using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record ScreenManagerState
    {
        public string Title { get; init; } = string.Empty;

        public bool IsTransitioning { get; init; }
    }

    public record SetScreenManagerTitleAction(string Title);
    
    public record ScreenManagerInitializedAction(ScreenManager ScreenManager);

    public record ScreenManagerTransitionAction(Type NextState, object? InterceptorAction = null, TimeSpan Delay = default);

    public record ScreenManagerTransitionEndAction();

    public static class ScreenManagerReducers
    {
        [ReducerMethod]
        public static ScreenManagerState OnSetScreenManagerTitle(ScreenManagerState state, SetScreenManagerTitleAction action) =>
            state with { Title = action.Title };

        [ReducerMethod(typeof(ScreenManagerTransitionAction))]
        public static ScreenManagerState OnScreenManagerTransition(ScreenManagerState state) =>
            state with { IsTransitioning = true };

        [ReducerMethod(typeof(ScreenManagerTransitionEndAction))]
        public static ScreenManagerState OnScreenManagerTransitionEnd(ScreenManagerState state) =>
            state with { IsTransitioning = false };
    }

    public class ScreenManagerEffects
    {
        private ScreenManager screenManager = default!;

        [EffectMethod]
        public Task OnScreenManagerInitialized(ScreenManagerInitializedAction action, IDispatcher dispatcher)
        {
            this.screenManager = action.ScreenManager;
            return Task.CompletedTask;
        }

        [EffectMethod]
        public async Task OnScreenManagerTransition(ScreenManagerTransitionAction action, IDispatcher dispatcher)
        {
            await this.screenManager.SetScreen(action.NextState, action.InterceptorAction, action.Delay);
            dispatcher.Dispatch(new ScreenManagerTransitionEndAction());
        }
    }
}