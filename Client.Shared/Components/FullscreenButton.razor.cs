using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class FullscreenButton
    {
        private bool Open => !this.ScreenService.IsStandalone;

        private bool IsInFullscreenMode =>
            this.ScreenService.IsInFullscreenMode;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.ScreenService.PropertyChanged += this.StateHasChanged;
        }

        private async Task OnButtonClick()
        {
            if (!this.ScreenService.RequestFullscreenEnabled)
            {
                await this.DialogService.ShowMessageBox(
                    this.L["Common.Fullscreen.Title"],
                    this.L["Common.Fullscreen.Message"]);
                return;
            }

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