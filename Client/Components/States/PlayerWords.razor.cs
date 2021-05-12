using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PlayerWords
    {
        [Parameter]
        public EventCallback<string[]> OnPlayerWordsSet { get; set; } = default!;

        public int WordCount {
            get => this.wordCount;
            set
            {
                this.wordCount = value;
                this.words = new string[value];
            } }

        private int wordCount = 2;

        private string[] words = new string[2];
    }
}