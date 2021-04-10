using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public abstract partial class State : ComponentBase
    {
        [CascadingParameter]
        public StateManager StateManager { get; set; } = default!;

        [Parameter]
        public TimeSpan Delay { get; set; }

        protected virtual RenderFragment Content => _ => {};

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private bool show = false;

        private string ShowClass => this.show ? "show" : string.Empty;

        public virtual async Task EnableAsync()
        {
            await Task.Delay(100);

            this.show = true;
            this.StateHasChanged();

            await Task.Delay(TransitionDuration);

            await Task.Delay(this.Delay);
        }

        public virtual async Task DisableAsync()
        {
            this.show = false;
            this.StateHasChanged();

            await Task.Delay(TransitionDuration);
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("Add " + this.GetType().Name);
            await this.StateManager.AddAsync(this);
            await base.OnInitializedAsync();
        }
    }
}