using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IScreenService
    {
        Task InitializeAsync() => Task.CompletedTask;

        ValueTask RequestWakeLock();

        ValueTask RequestFullScreen();
    }

    public class ScreenService : IScreenService
    {
        private const string JSModuleName = "ScreenModule";

        private readonly IJSRuntime js;

        private IJSObjectReference? module;

        private IJSObjectReference Module => this.module ?? throw new InvalidOperationException();

        public ScreenService(IJSRuntime js) => this.js = js;

        public async Task InitializeAsync() => this.module = await js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/screen.js");

        public ValueTask RequestWakeLock() => this.InvokeVoidAsync("requestWakeLock");

        public ValueTask RequestFullScreen() => this.InvokeVoidAsync("requestFullScreen");

        private ValueTask InvokeVoidAsync(string methodName) =>
            this.Module.InvokeVoidAsync($"{JSModuleName}.{methodName}");
    }

    public class DevScreenService : IScreenService
    {
        private readonly ILogger<DevScreenService> logger;

        public DevScreenService(ILogger<DevScreenService> logger) => this.logger = logger;

        public ValueTask RequestFullScreen()
        {
            this.logger.LogInformation("RequestFullScreen omitted in development mode.");
            return ValueTask.CompletedTask;
        }

        public ValueTask RequestWakeLock()
        {
            this.logger.LogInformation("RequestWakeLock omitted in development mode.");
            return ValueTask.CompletedTask;
        }
    }
}