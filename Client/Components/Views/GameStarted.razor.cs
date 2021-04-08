using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class GameStarted
    {
        [Parameter]
        public Team Team { get; set; } = default!;
    }
}