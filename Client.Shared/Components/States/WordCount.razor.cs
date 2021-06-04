using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class WordCount
    {
        [Parameter]
        public EventCallback<int> OnWordCountSet { get; set; } = default!;

        private int wordCount = 1;
    }
}