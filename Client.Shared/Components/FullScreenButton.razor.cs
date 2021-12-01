using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class FullScreenButton
    {
        private bool RequestFullScreenEnabled =>
            this.ScreenService.RequestFullScreenEnabled;

        private async Task RequestFullScreen()
        {
            await this.ScreenService.RequestFullScreen();
            await this.ScreenService.RequestWakeLock();
        }
    }
}