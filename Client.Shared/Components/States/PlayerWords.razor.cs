using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerWords
    {
        [Parameter]
        public EventCallback<string[]> OnPlayerWordsSet { get; set; } = default!;

        private int WordCount
        {
            get => this.wordCount;
            set
            {
                this.wordCount = value;
                this.words = new string[value];
            }
        }

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        private int wordCount = 2;

        private bool isValid = false;

        private string[] words = new string[2];

        public void Reset(int wordCount)
        {
            this.WordCount = wordCount;
            this.IsValid = false;
        }
    }
}