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

        public Task SetState(Type nextState)
        {
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync(nextState))
                .Unwrap();

            return this.transition;
        }

        private async Task TransitionAsync(Type nextState)
        {
            await this.DisableAsync();

            if (this.type == nextState)
            {
                this.type = null;
                this.StateHasChanged();
            }

            this.type = nextState;

            this.StateHasChanged();

            await this.EnableAsync();
        }

        private async Task EnableAsync()
        {
            await Task.Delay(100);

            this.show = true;

            this.StateHasChanged();

            await Task.Delay(TransitionDuration);
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