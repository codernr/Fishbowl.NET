using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Store;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class StateManager : IDisposable
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private Type? type;

        private bool show = false;

        private Task transition = Task.CompletedTask;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.StateManagerState.StateChanged += this.StateManagerStateChanged;
        }

        private async void StateManagerStateChanged(object? sender, StateManagerState state)
        {
            var newState = this.StateManagerState.Value;

            if (newState.CurrentState is null || !newState.IsTransitioning)
            {
                return;
            }
            
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync(newState.CurrentState))
                .Unwrap();

            await this.transition;
        }

        private async Task TransitionAsync(Type stateType)
        {
            await this.DisableAsync();

            if (this.type == stateType)
            {
                this.type = null;
                this.StateHasChanged();
            }

            this.type = stateType;

            this.StateHasChanged();

            await this.EnableAsync();

            this.Dispatcher.Dispatch(new StateManagerTransitionEndAction());
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

        public void Dispose() => this.StateManagerState.StateChanged -= this.StateManagerStateChanged;
    }
}