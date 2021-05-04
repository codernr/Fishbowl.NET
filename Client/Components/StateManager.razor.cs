using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Components
{
    public partial class StateManager
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        private Dictionary<Type, State> states = new();

        private Task transition = Task.CompletedTask;

        private State? activeState;

        private State ActiveState
        {
            get => this.activeState ?? throw new InvalidOperationException();
            set
            {
                this.activeState = value;
                this.StateHasChanged();
            }
        }

        public Task AddAsync(State state)
        {
            this.Logger.LogInformation("Add state: {StateType}", state.GetType().Name);
            this.states.Add(state.GetType(), state);
            state.Updated += this.StateHasChanged;

            if (this.states.Count == 1)
            {
                this.ActiveState = state;
                this.transition = this.transition
                    .ContinueWith(_ => this.ActiveState.EnableAsync())
                    .Unwrap();
            }

            return this.transition;
        }

        public void SetParameters<TState>(Action<TState> setParameters) where TState : State =>
            setParameters(this.GetState<TState>());

        public Task SetStateAsync<TState>(Action<TState>? setParameters = null) where TState : State
        {
            if (setParameters is not null) this.SetParameters(setParameters);

            this.transition = this.transition
                .ContinueWith(_ => this.TransitionAsync<TState>())
                .Unwrap();

            return this.transition;
        }

        private TState GetState<TState>() where TState : State => (TState)this.states[typeof(TState)];

        private async Task TransitionAsync<TState>() where TState : State
        {
            var newState = this.GetState<TState>();

            if (newState == this.ActiveState) return;

            await this.ActiveState.DisableAsync();

            this.ActiveState = newState;
            
            await this.ActiveState.EnableAsync();
        }
    }
}