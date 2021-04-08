using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class RoundStarted
    {
        [Parameter]
        public Round Round { get; set; } = default!;
    }
}