using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class WordCount
    {
        [Parameter]
        public EventCallback<int> OnWordCountSet { get; set; } = default!;

        private MudForm? form;

        private int wordCount = 1;
    }
}