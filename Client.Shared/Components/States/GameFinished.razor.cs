using System;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class GameFinished
    {
        public Action ReloadRequested { get; set; } = default!;
        
        public GameSummaryViewModel Game { get; set; } = default!;
    }
}