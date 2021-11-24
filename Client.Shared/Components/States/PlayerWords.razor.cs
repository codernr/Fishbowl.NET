using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerWords
    {
        public Func<string[], Task> OnPlayerWordsSet { get; set; } = default!;

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
                this.StateHasChanged();
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