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

        [Parameter]
        public bool Show { get; set; } = false;

        public event Action? Updated;

        protected virtual RenderFragment Content => _ => {};

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private string ShowClass => this.Show ? "show" : string.Empty;

        public virtual async Task EnableAsync()
        {
            if (this.Show) return;
            
            await Task.Delay(100);

            this.Show = true;
            this.Update();

            await Task.Delay(TransitionDuration);

            await Task.Delay(this.Delay);
        }

        public virtual async Task DisableAsync()
        {
            this.Show = false;
            this.Update();

            await Task.Delay(TransitionDuration);
        }

        protected void Update() => this.Updated?.Invoke();

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("Add " + this.GetType().Name);
            await this.StateManager.AddAsync(this);
            await base.OnInitializedAsync();
        }
    }
}