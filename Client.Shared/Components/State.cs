using System;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class State<T> : ComponentBase where T : State<T>
    {
        [Parameter]
        public Action<T>? SetParameters { get; set; }

        public void Update(Action<T> updateParameters)
        {
            updateParameters(this.Instance);
            this.StateHasChanged();
        }

        private T Instance => this as T ?? throw new InvalidCastException();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.SetParameters?.Invoke(this.Instance);
        }
    }
}