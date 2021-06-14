using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class StateManager
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [Parameter]
        public Action<State>? TransitionStarted { get; set; }

        private Dictionary<Type, State> states = new();

        private Task transition = Task.CompletedTask;

        private State? activeState;

        private State? ActiveState
        {
            get => this.activeState;
            set
            {
                this.activeState = value;
                this.StateHasChanged();
            }
        }

        public void Add(State state)
        {
            this.Logger.LogInformation("Add state: {StateType}", state.GetType().Name);
            this.states.Add(state.GetType(), state);
            state.Updated += this.StateHasChanged;
        }

        public void SetParameters<TState>(Action<TState> setParameters) where TState : State
        {
            var state = this.GetState<TState>();
            setParameters(state);
            this.StateHasChanged();
        }

        public Task SetStateAsync<TState>(Action<TState>? setParameters = null) where TState : State
        {
            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync<TState>(setParameters ?? (_ => {})))
                .Unwrap();

            return this.transition;
        }

        private TState GetState<TState>() where TState : State => (TState)this.states[typeof(TState)];

        private async Task TransitionAsync<TState>(Action<TState> setParameters) where TState : State
        {
            var newState = this.GetState<TState>();

            this.TransitionStarted?.Invoke(newState);

            if (this.ActiveState is not null) await this.ActiveState.DisableAsync();

            setParameters(newState);

            this.ActiveState = newState;
            
            await this.ActiveState.EnableAsync();
        }
    }
}