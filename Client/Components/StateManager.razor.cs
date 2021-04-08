using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components
{
    public partial class StateManager
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        private Dictionary<GameState, State> states = new();

        private Task transition = Task.CompletedTask;

        private GameState activeState;

        public Task AddStateAsync(GameState state, State item)
        {
            this.states.Add(state, item);

            if (state == this.activeState)
            {
                this.transition = this.transition
                    .ContinueWith(_ => this.states[this.activeState].Enable())
                    .Unwrap();
            }

            return this.transition;
        }

        public Task SetState(GameState state)
        {
            this.transition = this.transition
                .ContinueWith(_ => this.states[this.activeState].Disable())
                .Unwrap()
                .ContinueWith(_ =>
                {
                    this.activeState = state;
                    return this.states[this.activeState].Enable();
                })
                .Unwrap();

            return this.transition;
        }
    }
}