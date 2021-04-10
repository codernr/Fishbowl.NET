using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PlayerWords
    {
        [Parameter]
        public EventCallback<string[]> OnPlayerWordsSet { get; set; } = default!;

        private string[] words = new string[2];
    }
}