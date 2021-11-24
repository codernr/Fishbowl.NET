using System;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class ConnectionClosed
    {
        public Action ReloadRequested { get; set; } = default!;
    }
}