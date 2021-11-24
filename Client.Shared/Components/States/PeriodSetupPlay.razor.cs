using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodSetupPlay
    {
        public Func<DateTimeOffset, Task> OnStarted { get; set; } = default!;

        public PeriodSetupViewModel Period { get; set; } = default!;

        private Once once = new();

        private Task Start() =>
            this.once.Fire(() => this.OnStarted(DateTimeOffset.UtcNow));
    }
}