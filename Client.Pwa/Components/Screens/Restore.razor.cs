using Fishbowl.Net.Client.Pwa.Store;

namespace Fishbowl.Net.Client.Pwa.Components.Screens
{
    public partial class Restore
    {
        private void RequestRestore() => this.DispatchOnce<RestoreGameAction>(new());

        private void RequestNewGame() => this.DispatchOnce<StartNewGameAction>(new());
    }
}