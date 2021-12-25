using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record ScreenState
    {
        public bool RequestFullscreenEnabled { get; init; }
        public bool IsInFullscreenMode { get; init; }
        public bool IsStandalone { get; init; }
    }

    public record SetScreenRequestFullscreenEnabledAction(bool RequestFullScreenEnabled);

    public record SetScreenIsStandaloneAction(bool IsStandalone);

    public record SetScreenIsInFullscreenModeAction(bool IsInFullscreenMode);

    public record ScreenRequestFullscreenAction();

    public record ScreenExitFullscreenAction();

    public static class ScreenReducers
    {
        [ReducerMethod]
        public static ScreenState OnSetScreenRequestFullscreenEnabled(ScreenState state, SetScreenRequestFullscreenEnabledAction action) =>
            state with { RequestFullscreenEnabled = action.RequestFullScreenEnabled };
                
        [ReducerMethod]
        public static ScreenState OnSetScreenIsInFullscreenMode(ScreenState state, SetScreenIsInFullscreenModeAction action) =>     
            state with { IsInFullscreenMode = action.IsInFullscreenMode };
                
        [ReducerMethod]
        public static ScreenState OnSetScreenIsStandalone(ScreenState state, SetScreenIsStandaloneAction action) =>
            state with { IsStandalone = action.IsStandalone };
    }

    public class ScreenEffects : IDisposable
    {
        private const string JSModuleName = "ScreenModule";

        private readonly IJSRuntime js;

        private readonly DotNetObjectReference<ScreenEffects> objectReference;

        private IJSObjectReference? module;

        private IJSObjectReference Module => this.module ?? throw new InvalidOperationException();

        private IDispatcher dispatcher = default!;
        
        public ScreenEffects(IJSRuntime js) =>
            (this.js, this.objectReference) = (js, DotNetObjectReference.Create(this));

        [EffectMethod(typeof(StoreInitializedAction))]
        public async Task InitializeAsync(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            this.module = await js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/screen.js");

            await this.InvokeVoidAsync("initialize", this.objectReference);

            dispatcher.Dispatch(
                new SetScreenRequestFullscreenEnabledAction(await this.InvokeAsync<bool>("requestFullscreenEnabled")));
            dispatcher.Dispatch(
                new SetScreenIsStandaloneAction(await this.InvokeAsync<bool>("isStandalone")));
        }

        [EffectMethod(typeof(ScreenRequestFullscreenAction))]
        public Task OnScreenRequestFullscreen(IDispatcher dispatcher) =>
            Task.WhenAll(
                this.InvokeVoidAsync("requestFullscreen").AsTask(),
                this.InvokeVoidAsync("requestWakeLock").AsTask());

        [EffectMethod(typeof(ScreenExitFullscreenAction))]
        public Task OnScreenExitFullscreen(IDispatcher dispatcher) =>
            this.InvokeVoidAsync("exitFullscreen").AsTask();

        [JSInvokable]
        public void OnFullscreenChange(bool isInFullscreenMode) =>
            this.dispatcher.Dispatch(new SetScreenIsInFullscreenModeAction(isInFullscreenMode));

        [JSInvokable]
        public void OnStandaloneChange(bool isStandalone) =>
            this.dispatcher.Dispatch(new SetScreenIsStandaloneAction(isStandalone));

        private ValueTask InvokeVoidAsync(string methodName, params object?[]? args) =>
            this.Module.InvokeVoidAsync($"{JSModuleName}.{methodName}", args);

        private ValueTask<T> InvokeAsync<T>(string methodName) =>
            this.Module.InvokeAsync<T>($"{JSModuleName}.{methodName}");

        public void Dispose()
        {
            this.objectReference.Dispose();
        }
    }
}