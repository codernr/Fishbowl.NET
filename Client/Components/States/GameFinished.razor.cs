using Fishbowl.Net.Shared.Data.ViewModels;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class GameFinished
    {
        public GameSummaryViewModel Game { get; set; } = default!;
    }
}