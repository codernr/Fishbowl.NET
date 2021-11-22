using System;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class Password
    {
        [Parameter]
        public EventCallback<string> OnJoinGame { get; set; } = default!;

        [Parameter]
        public EventCallback<string> OnCreateGame { get; set; } = default!;

        private bool IsValid
        {
            get => this.isValid;
            set
            {
                this.isValid = value;
                this.Update();
            }
        }

        private bool isValid = false;

        private string password = string.Empty;
    }
}