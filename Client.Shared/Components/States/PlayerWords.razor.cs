using System;
using System.Threading.Tasks;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerWords
    {
        public Func<string[], Task> OnPlayerWordsSet { get; set; } = default!;

        public int WordCount
        {
            get => this.wordCount;
            set
            {
                this.wordCount = value;
                this.words = new string[value];
            }
        }

        private MudForm? form;

        private int wordCount = 2;

        private string[] words = new string[2];
    }
}