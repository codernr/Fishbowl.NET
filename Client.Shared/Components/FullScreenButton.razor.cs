using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class FullScreenButton
    {
        private bool RequestFullScreenEnabled =>
            this.ScreenService.RequestFullScreenEnabled;

        private bool IsInFullScreenMode =>
            this.ScreenService.IsInFullScreenMode;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.ScreenService.PropertyChanged += this.StateHasChanged;
        }

        private async Task Switch()
        {
            if (this.IsInFullScreenMode)
            {
                await this.ScreenService.ExitFullScreen();
            }
            else
            {
                await this.ScreenService.RequestFullScreen();
                await this.ScreenService.RequestWakeLock();
            }
        }
    }
}