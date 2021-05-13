using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.Data.ViewModels;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class WaitingForTeamNames
    {
        public TeamViewModel Team { get; set; } = default!;
        
        public IEnumerable<TeamViewModel> Teams { get; set; } = Enumerable.Empty<TeamViewModel>();
    }
}