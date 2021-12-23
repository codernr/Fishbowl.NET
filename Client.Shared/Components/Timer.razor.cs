using System;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class Timer
    {
        private string Classes => this.State.Value.Remaining > TimeSpan.FromSeconds(15) ?
            "mud-theme-info" : (this.State.Value.Remaining > TimeSpan.FromSeconds(5) ? "mud-theme-warning" : "mud-theme-error");

        private string TimeFormat =>
            string.Format("{0}{1:mm\\:ss}", this.Prefix, this.State.Value.Remaining);

        private string Prefix => this.State.Value.Remaining < TimeSpan.Zero ? "-" : string.Empty;
    }
}