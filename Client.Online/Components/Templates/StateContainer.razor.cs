using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components.Templates
{
    public partial class StateContainer
    {
        [Parameter]
        public string Title { get; set; } = default!;

        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
    }
}