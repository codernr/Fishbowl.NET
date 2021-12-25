using System;
using Fishbowl.Net.Client.Shared.Store;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class ConnectionClosed
    {
        private void Reload() => this.Dispatcher.Dispatch(new ReloadAction());
    }
}