using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IScreenService
    {
        event Action? PropertyChanged;

        bool RequestFullScreenEnabled { get; }

        bool IsInFullScreenMode { get; }

        Task InitializeAsync();

        ValueTask RequestWakeLock();

        ValueTask RequestFullScreen();

        ValueTask ExitFullScreen();
    }

    public class ScreenService : IScreenService, IDisposable
    {
        public event Action? PropertyChanged;

        public bool RequestFullScreenEnabled { get; private set; }

        public bool IsInFullScreenMode { get; private set; }

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

            this.RequestFullScreenEnabled = await this.InvokeAsync<bool>("requestFullScreenEnabled");

            await this.InvokeVoidAsync("initialize", this.objectReference);
        }

        public ValueTask RequestWakeLock() => this.InvokeVoidAsync("requestWakeLock");

        public ValueTask RequestFullScreen() => this.InvokeVoidAsync("requestFullScreen");

        public ValueTask ExitFullScreen() => this.InvokeVoidAsync("exitFullScreen");

        [JSInvokable]
        public void OnFullScreenChange(bool isInFullScreenMode)
        {
            this.IsInFullScreenMode = isInFullScreenMode;
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