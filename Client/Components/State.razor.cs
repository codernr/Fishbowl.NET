using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class State
    {
        [CascadingParameter]
        public StateManager StateManager { get; set; } = default!;

        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [Parameter]
        public GameState StateType { get; set; }

        [Parameter]
        public TimeSpan Delay { get; set; }

        private bool enabled = false;

        private bool show = false;

        private string ShowClass => this.show ? "show" : string.Empty;

        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("Add " + this.StateType);
            await this.StateManager.AddStateAsync(this.StateType, this);
            await base.OnInitializedAsync();
        }

        public async Task Enable()
        {
            if (this.enabled) return;

            this.enabled = true;
            this.StateHasChanged();

            await Task.Delay(100);

            this.show = true;
            this.StateHasChanged();

            await Task.Delay(TransitionDuration);

            await Task.Delay(this.Delay);
        }

        public async Task Disable()
        {
            if (!this.enabled) return;

            this.show = false;
            this.StateHasChanged();

            await Task.Delay(TransitionDuration);

            this.enabled = false;
            this.StateHasChanged();
        }
    }
}