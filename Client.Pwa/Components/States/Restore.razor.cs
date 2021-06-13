using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class Restore
    {
        [Parameter]
        public EventCallback OnRestoreRequested { get; set; } = default!;

        [Parameter]
        public EventCallback OnNewGameRequested { get; set; } = default!;
    }
}