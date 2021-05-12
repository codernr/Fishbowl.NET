using System;
using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PeriodSetupPlay
    {
        [Parameter]
        public EventCallback<DateTimeOffset> OnStarted { get; set; } = default!;

        public PeriodSetupViewModel Period { get; set; } = default!;
    }
}