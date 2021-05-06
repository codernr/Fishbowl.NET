using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class ToastContainer : ComponentBase
    {
        private readonly List<string> messages = new();

        private int animating = 0;

        public void DisplayToast(string message)
        {
            this.messages.Add(message);
            this.animating++;
            this.StateHasChanged();
        }

        private void AnimationFinished()
        {
            if (--this.animating == 0) this.messages.Clear();
        }
    }
}