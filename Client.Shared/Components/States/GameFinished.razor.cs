using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class GameFinished
    {
        [Parameter]
        public EventCallback ReloadRequested { get; set; } = default!;
        
        public GameSummaryViewModel Game { get; set; } = default!;
    }
}