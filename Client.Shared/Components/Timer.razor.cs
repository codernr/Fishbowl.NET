using System;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class Timer
    {
        [Parameter]
        public TimeSpan Remaining { get; set; }

        private string Classes => this.Remaining > TimeSpan.FromSeconds(15) ?
            "mud-theme-info" : (this.Remaining > TimeSpan.FromSeconds(5) ? "mud-theme-warning" : "mud-theme-error");

        private string TimeFormat =>
            string.Format("{0}{1:mm\\:ss}", this.Prefix, this.Remaining);

        private string Prefix => this.Remaining < TimeSpan.Zero ? "-" : string.Empty;
    }
}