using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class ToastContainer : ComponentBase
    {
        private readonly List<(string message, string contextClass)> toasts = new();

        private int animating = 0;

        public void DisplayToast(string message, string contextClass)
        {
            this.toasts.Add((message, contextClass));
            this.animating++;
            this.StateHasChanged();
        }

        private void AnimationFinished()
        {
            if (--this.animating == 0) this.toasts.Clear();
        }
    }
}