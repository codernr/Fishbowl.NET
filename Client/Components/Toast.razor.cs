using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class Toast : ComponentBase
    {
        public enum ToastType
        {
            Primary,
            Warning
        }

        private static readonly Dictionary<ToastType, string> Classes = new()
        {
            { ToastType.Primary, "text-light bg-primary" },
            { ToastType.Warning, "text-dark bg-warning" }
        };

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(150);

        private static readonly TimeSpan Delay = TimeSpan.FromSeconds(2);

        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public ToastType Type { get; set; } = ToastType.Primary;

        [Parameter]
        public bool Auto { get; set; } = true;

        [Parameter]
        public EventCallback AnimationFinished { get; set; } = default!;

        private string animationClass = "hide";

        protected override async Task OnInitializedAsync()
        {
            if (!this.Auto) return;
            
            await this.Show();

            await Task.Delay(Delay);
            
            await this.Hide();

            await this.AnimationFinished.InvokeAsync();
        }

        public async Task Show()
        {
            await Task.Delay(100);
            this.animationClass = "showing show";
            this.StateHasChanged();

            await Task.Delay(TransitionDuration);
            this.animationClass = "show";
            this.StateHasChanged();
        }

        public async Task Hide()
        {
            this.animationClass = string.Empty;
            this.StateHasChanged();
            await Task.Delay(TransitionDuration);

            this.animationClass = "hide";
            this.StateHasChanged();
        }
    }
}