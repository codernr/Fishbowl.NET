using System;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodSetupPlay
    {
        public Action<DateTimeOffset> OnStarted { get; set; } = default!;

        public PeriodSetupViewModel Period { get; set; } = default!;
    }
}