using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.Templates
{
    public partial class ScreenContainer
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
    }
}