using Fishbowl.Net.Client.Pwa.Store;

namespace Fishbowl.Net.Client.Pwa.Components.Screens
{
    public partial class Restore
    {
        private void RequestRestore() =>
            this.Dispatcher.Dispatch(new RestoreGameAction());

        private void RequestNewGame() =>
            this.Dispatcher.Dispatch(new StartNewGameAction());
    }
}