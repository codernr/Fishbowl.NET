using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components
{
    public partial class Toast : ComponentBase
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(150);

        private static readonly TimeSpan Delay = TimeSpan.FromSeconds(2);

        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public string ContextClass { get; set; } = ContextCssClass.Primary;

        [Parameter]
        public bool Auto { get; set; } = true;

        [Parameter]
        public Action? AnimationFinished { get; set; } = default!;

        private bool Visible => this.animationClass.Contains("show");

        private string animationClass = "hide";

        private Task transition = Task.CompletedTask;

        protected override async Task OnInitializedAsync()
        {
            if (!this.Auto) return;
            
            await this.Show();

            await Task.Delay(Delay);
            
            await this.Hide();

            this.AnimationFinished?.Invoke();
        }

        public Task Show()
        {
            if (this.Visible) return Task.CompletedTask;

            this.transition = this.transition
                .ContinueWith(async _ =>
                {
                    this.animationClass = string.Empty;
                    this.StateHasChanged();
                    await Task.Delay(100);
                    
                    this.animationClass = "showing show";
                    this.StateHasChanged();

                    await Task.Delay(TransitionDuration);
                    this.animationClass = "show";
                    this.StateHasChanged();
                })
                .Unwrap();

            return this.transition;
        }

        public Task Hide()
        {
            if (!this.Visible) return Task.CompletedTask;
            
            this.transition = this.transition
                .ContinueWith(async _ =>
                {
                    this.animationClass = string.Empty;
                    this.StateHasChanged();
                    await Task.Delay(TransitionDuration);

                    this.animationClass = "hide";
                    this.StateHasChanged();
                })
                .Unwrap();

            return this.transition;
        }
    }
}