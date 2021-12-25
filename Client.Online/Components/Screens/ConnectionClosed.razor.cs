using Fishbowl.Net.Client.Shared.Store;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class ConnectionClosed
    {
        private void Reload() => this.Dispatcher.Dispatch(new ReloadAction());
    }
}