using System.Threading.Tasks;
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
}