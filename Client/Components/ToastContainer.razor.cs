using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class ToastContainer : ComponentBase
    {
        private readonly List<(string message, Toast.ToastType type)> toasts = new();

        private int animating = 0;

        public void DisplayToast(string message, Toast.ToastType type)
        {
            this.toasts.Add((message, type));
            this.animating++;
            this.StateHasChanged();
        }

        private void AnimationFinished()
        {
            if (--this.animating == 0) this.toasts.Clear();
        }
    }
}