using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodSetupWatch
    {
        [Parameter]
        public Round Round { get; set; } = default!;

        [Parameter]
        public Period Period { get; set; } = default!;
    }

}