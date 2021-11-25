using System.Collections.Generic;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class WaitingForTeamNames
    {
        public PlayerViewModel? SetupPlayer { get; set; }

        public TeamViewModel Team { get; set; } = default!;
        
        public ICollection<TeamViewModel> Teams { get; set; } = default!;
    }
}