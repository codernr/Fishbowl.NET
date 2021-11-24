using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerSetup
    {
        public record Model(string PlayerName, string[] Words);

        public Func<PlayerSetup.Model, Task> OnPlayerSetup { get; set; } = default!;

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

        private string playerName = string.Empty;

        private int wordCount = 2;

        private string[] words = new string[2];

        private Task Submit() =>
            this.once.Fire(() => this.OnPlayerSetup(new(this.playerName, this.words)));
    }
}