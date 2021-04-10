using System;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PeriodSetupPlay
    {
        [Parameter]
        public EventCallback<DateTimeOffset> OnStarted { get; set; } = default!;
        
        public Round Round { get; set; } = default!;
    }
}