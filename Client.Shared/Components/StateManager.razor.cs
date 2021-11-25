using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class StateManager
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

        private DynamicComponent Component => this.component ?? throw new InvalidOperationException();

        private DynamicComponent? component; 

        private Type? type;

        private Dictionary<string, object> parameters = new();

        private bool show = false;

        private Task transition = Task.CompletedTask;

        private T Instance<T>() where T : State<T> =>
            this.Component.Instance as T ?? throw new InvalidOperationException();

        public void SetParameters<T>(Action<T> setParameters) where T : State<T> =>
            this.Instance<T>().Update(setParameters);

        public Task SetStateAsync<T>(Action<T>? setParameters = null, TimeSpan delay = default) where T : State<T>
        {
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync<T>(setParameters ?? (_ => {}), delay))
                .Unwrap();

            return this.transition;
        }

        private async Task TransitionAsync<T>(Action<T> setParameters, TimeSpan delay = default) where T : State<T>
        {
            await this.DisableAsync();

            if (this.type == typeof(T))
            {
                this.type = null;
                this.StateHasChanged();
            }

            this.type = typeof(T);
            this.parameters = new() { { "SetParameters", setParameters } };

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