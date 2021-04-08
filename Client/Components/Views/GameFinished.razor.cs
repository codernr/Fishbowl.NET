using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class GameFinished
    {
        [Parameter]
        public Game Game { get; set; } = default!;
    }
}