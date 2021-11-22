using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerName
    {
        [Parameter]
        public EventCallback<string> OnPlayerNameSet { get; set; } = default!;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        public string Value { get; set; } = string.Empty;

        private bool isValid = false;

        public void Reset()
        {
            this.Value = string.Empty;
            this.IsValid = false;
        }
    }
}