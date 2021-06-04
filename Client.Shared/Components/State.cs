using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract partial class State : ComponentBase
    {
        [CascadingParameter]
        public StateManager StateManager { get; set; } = default!;

        [Parameter]
        public TimeSpan Delay { get; set; }

        public event Action? Updated;

        protected virtual RenderFragment Content => _ => {};

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private bool show = false;

        private string ShowClass => this.show ? "show" : string.Empty;

        public virtual async Task EnableAsync()
        {
            await Task.Delay(100);

            this.show = true;
            this.Update();

            await Task.Delay(TransitionDuration);

            await Task.Delay(this.Delay);
        }

        public virtual async Task DisableAsync()
        {
            this.show = false;
            this.Update();

            await Task.Delay(TransitionDuration);
        }

        protected void Update() => this.Updated?.Invoke();

        protected override async Task OnInitializedAsync()
        {
            await this.StateManager.AddAsync(this);
            await base.OnInitializedAsync();
        }
    }
}