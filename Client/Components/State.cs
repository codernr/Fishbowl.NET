using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public abstract class State : ComponentBase
    {
        [CascadingParameter]
        public StateManager StateManager { get; set; } = default!;

        [Parameter]
        public TimeSpan Delay { get; set; }

        public virtual RenderFragment Content => _ => {};

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("Add " + this.GetType().Name);
            await this.StateManager.AddAsync(this);
            await base.OnInitializedAsync();
        }
    }
}