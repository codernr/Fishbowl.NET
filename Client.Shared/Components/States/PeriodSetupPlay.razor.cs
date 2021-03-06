using System;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodSetupPlay
    {
        [Parameter]
        public EventCallback<DateTimeOffset> OnStarted { get; set; } = default!;

        public PeriodSetupViewModel Period { get; set; } = default!;
    }
}