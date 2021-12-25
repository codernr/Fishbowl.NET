using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class StateManager
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private Type? type;

        private bool show = false;

        private Task transition = Task.CompletedTask;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.StateManagerService.Initialize(this);
        }

        public Task SetState(Type nextState, TimeSpan delay = default)
        {
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync(nextState, delay))
                .Unwrap();

            return this.transition;
        }

        private async Task TransitionAsync(Type nextState, TimeSpan delay)
        {
            await this.DisableAsync();

            if (this.type == nextState)
            {
                this.type = null;
                this.StateHasChanged();
            }

            this.type = nextState;

            this.StateHasChanged();

            await this.EnableAsync(delay);
        }

        private async Task EnableAsync(TimeSpan delay)
        {
            await Task.Delay(100);

            this.show = true;

            this.StateHasChanged();

            await Task.Delay(TransitionDuration + delay);
        }

        private async Task DisableAsync()
        {
            if (this.type is null)
            {
                return;
            }

            this.show = false;

            this.StateHasChanged();

            await Task.Delay(TransitionDuration);
        }
    }
}