using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerName
    {
        public Func<string, Task> OnPlayerNameSet { get; set; } = default!;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        private string Value { get; set; } = string.Empty;

        private bool isValid = false;

        public void Reset()
        {
            this.Value = string.Empty;
            this.IsValid = false;
        }
    }
}