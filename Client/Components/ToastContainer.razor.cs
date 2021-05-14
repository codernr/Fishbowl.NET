using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class ToastContainer : ComponentBase
    {
        private readonly Queue<(Guid key, string message, string contextClass)> toasts = new();

        private int animating = 0;

        public void DisplayToast(string message, string contextClass)
        {
            this.toasts.Enqueue((Guid.NewGuid(), message, contextClass));
            this.animating++;
            this.StateHasChanged();
        }

        private void AnimationFinished()
        {
            this.toasts.Dequeue();
            this.StateHasChanged();
        }
    }
}