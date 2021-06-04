using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IScreenService
    {
        ValueTask RequestWakeLock();

        ValueTask RequestFullScreen();
    }

    public class ScreenService : IScreenService
    {
        private const string JSModuleName = "ScreenModule";

        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public ScreenService(IJSRuntime js) =>
            this.moduleTask = new (() => js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/screen.js").AsTask());

        public ValueTask RequestWakeLock() => this.InvokeVoidAsync("requestWakeLock");

        public ValueTask RequestFullScreen() => this.InvokeVoidAsync("requestFullScreen");

        private async ValueTask InvokeVoidAsync(string methodName)
        {
            var module = await this.moduleTask.Value;
            await module.InvokeVoidAsync($"{JSModuleName}.{methodName}");
        }
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