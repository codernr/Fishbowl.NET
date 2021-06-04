using System.Collections.Generic;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components
{
    public partial class TeamMembers : ComponentBase
    {
        [Parameter]
        public IEnumerable<PlayerViewModel> Players { get; set; } = default!;
    }
}