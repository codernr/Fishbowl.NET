using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
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

        private Once once = new();

        private int wordCount = 2;

        private string[] words = new string[2];

        private Task Submit() =>
            this.once.Fire(() => this.OnPlayerWordsSet(this.words));
    }
}