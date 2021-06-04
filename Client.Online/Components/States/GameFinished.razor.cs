using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class GameFinished
    {
        [Parameter]
        public EventCallback ReloadRequested { get; set; } = default!;
        
        public GameSummaryViewModel Game { get; set; } = default!;

        public bool Winner { get; set; } = false;
    }
}