using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Store;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class FullscreenButton
    {
        private async Task OnButtonClick()
        {
            if (!this.State.Value.RequestFullscreenEnabled)
            {
                await this.DialogService.ShowMessageBox(
                    this.L["Common.Fullscreen.Title"],
                    this.L["Common.Fullscreen.Message"]);
                return;
            }

            this.Dispatcher.Dispatch(this.State.Value.IsInFullscreenMode ?
                new ScreenExitFullscreenAction() :
                new ScreenRequestFullscreenAction());
        }
    }
}