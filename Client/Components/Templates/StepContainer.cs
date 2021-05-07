using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Templates
{
    public partial class StepContainer
    {
        [Parameter]
        public string Title { get; set; } = default!;

        [Parameter]
        public string Subtitle { get; set; } = default!;

        [Parameter]
        public int TotalSteps { get; set; }
        
        [Parameter]
        public int Step { get; set; } = 1;

        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
    }
}