using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IStateManagerService
    {
        void Initialize(StateManager component);

        Task SetState(Type nextState);
    }

    public class StateManagerService : IStateManagerService
    {
        private StateManager component = default!;

        public void Initialize(StateManager component) => this.component = component;

        public Task SetState(Type nextState) => this.component.SetState(nextState);
    }
}