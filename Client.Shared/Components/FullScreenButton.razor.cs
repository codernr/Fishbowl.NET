using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class FullscreenButton
    {
        private bool Open =>
            this.ScreenService.RequestFullscreenEnabled && !this.ScreenService.IsStandalone;

        private bool IsInFullscreenMode =>
            this.ScreenService.IsInFullscreenMode;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.ScreenService.PropertyChanged += this.StateHasChanged;
        }

        private async Task Switch()
        {
            if (this.IsInFullscreenMode)
            {
                await this.ScreenService.ExitFullscreen();
            }
            else
            {
                await this.ScreenService.RequestFullscreen();
                await this.ScreenService.RequestWakeLock();
            }
        }
    }
}