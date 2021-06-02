using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Services
{
    public interface IScreenService
    {
        ValueTask RequestWakeLock();

        ValueTask RequestFullScreen();
    }

    public class ScreenService : IScreenService
    {
        private readonly IJSRuntime js;

        public ScreenService(IJSRuntime js) => this.js = js;

        public ValueTask RequestWakeLock() => this.js.InvokeVoidAsync("ScreenModule.requestWakeLock");

        public ValueTask RequestFullScreen() => this.js.InvokeVoidAsync("ScreenModule.requestFullScreen");
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