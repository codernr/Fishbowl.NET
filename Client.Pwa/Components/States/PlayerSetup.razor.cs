using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerSetup
    {
        public record PlayerData(string Name, string[] Words);

        [Parameter]
        public EventCallback<PlayerData> OnPlayerSetup { get; set; } = default!;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        private int WordCount
        {
            get => this.wordCount;
            set
            {
                this.wordCount = value;
                this.words = new string[value];
            }
        }

        private string Name { get; set; } = string.Empty;

        private bool isValid = false;

        private int wordCount = 2;

        private string[] words = new string[2];

        public void Reset(int wordCount)
        {
            this.Name = string.Empty;
            this.WordCount = wordCount;
            this.IsValid = false;
        }

        private Task SetupPlayer() =>
            this.Submit(() => this.OnPlayerSetup.InvokeAsync(new(this.Name, this.words)));
    }
}