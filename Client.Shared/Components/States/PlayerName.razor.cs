using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PlayerName
    {
        [Parameter]
        public EventCallback<string> OnPlayerNameSet { get; set; } = default!;

        private bool isValid = false;

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
    }
}