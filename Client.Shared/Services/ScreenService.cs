using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IScreenService
    {
        event Action? PropertyChanged;

        bool RequestFullscreenEnabled { get; }

        bool IsInFullscreenMode { get; }

        Task InitializeAsync();

        ValueTask RequestWakeLock();

        ValueTask RequestFullscreen();

        ValueTask ExitFullscreen();
    }

    public class ScreenService : IScreenService, IDisposable
    {
        public event Action? PropertyChanged;

        public bool RequestFullscreenEnabled { get; private set; }

        public bool IsInFullscreenMode { get; private set; }

        private const string JSModuleName = "ScreenModule";

        private readonly IJSRuntime js;

        private readonly DotNetObjectReference<ScreenService> objectReference;

        private IJSObjectReference? module;

        private IJSObjectReference Module => this.module ?? throw new InvalidOperationException();

        public ScreenService(IJSRuntime js) =>
            (this.js, this.objectReference) = (js, DotNetObjectReference.Create(this));

        public async Task InitializeAsync()
        {
            this.module = await js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/screen.js");

            this.RequestFullscreenEnabled = await this.InvokeAsync<bool>("requestFullscreenEnabled");

            await this.InvokeVoidAsync("initialize", this.objectReference);
        }

        public ValueTask RequestWakeLock() => this.InvokeVoidAsync("requestWakeLock");

        public ValueTask RequestFullscreen() => this.InvokeVoidAsync("requestFullscreen");

        public ValueTask ExitFullscreen() => this.InvokeVoidAsync("exitFullscreen");

        [JSInvokable]
        public void OnFullscreenChange(bool isInFullscreenMode)
        {
            this.IsInFullscreenMode = isInFullscreenMode;
            this.PropertyChanged?.Invoke();
        }

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