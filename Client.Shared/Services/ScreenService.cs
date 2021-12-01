using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IScreenService
    {
        bool RequestFullScreenEnabled { get; }

        Task InitializeAsync();

        ValueTask RequestWakeLock();

        ValueTask RequestFullScreen();
    }

    public class ScreenService : IScreenService
    {
        public bool RequestFullScreenEnabled { get; private set; }

        private const string JSModuleName = "ScreenModule";

        private readonly IJSRuntime js;

        private IJSObjectReference? module;

        private IJSObjectReference Module => this.module ?? throw new InvalidOperationException();

        public ScreenService(IJSRuntime js) => this.js = js;

        public async Task InitializeAsync()
        {
            this.module = await js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/screen.js");

            this.RequestFullScreenEnabled = await this.InvokeAsync<bool>("requestFullScreenEnabled");
        }

        public ValueTask RequestWakeLock() => this.InvokeVoidAsync("requestWakeLock");

        public ValueTask RequestFullScreen() => this.InvokeVoidAsync("requestFullScreen");

        private ValueTask InvokeVoidAsync(string methodName) =>
            this.Module.InvokeVoidAsync($"{JSModuleName}.{methodName}");

        private ValueTask<T> InvokeAsync<T>(string methodName) =>
            this.Module.InvokeAsync<T>($"{JSModuleName}.{methodName}");
    }
}