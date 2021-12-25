using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Store;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class ScreenManager
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private Type? type;

        private bool show = false;

        private Task transition = Task.CompletedTask;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Dispatcher.Dispatch(new ScreenManagerInitializedAction(this));
        }

        public Task SetScreen(Type screen, object? interceptorAction = null, TimeSpan delay = default)
        {
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync(screen, interceptorAction, delay))
                .Unwrap();

            return this.transition;
        }

        private async Task TransitionAsync(Type screen, object? interceptorAction, TimeSpan delay)
        {
            await this.DisableAsync();

            if (this.type == screen)
            {
                this.type = null;
                this.StateHasChanged();
            }

            if (interceptorAction is not null)
            {
                this.Dispatcher.Dispatch(interceptorAction);
            }

            this.type = screen;

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