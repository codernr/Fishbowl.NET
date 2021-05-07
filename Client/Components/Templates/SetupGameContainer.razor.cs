using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Templates
{
    public partial class SetupGameContainer
    {
        [Parameter]
        public string Subtitle { get; set; } = default!;

        [Parameter]
        public int Step { get; set; } = 1;

        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
    }
}