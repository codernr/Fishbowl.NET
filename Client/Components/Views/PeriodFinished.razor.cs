using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PeriodFinished
    {
        [Parameter]
        public Period Period { get; set; } = default!;
    }
}