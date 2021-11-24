using System;
using Fishbowl.Net.Client.Shared.I18n;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class State<T> : ComponentBase where T : State<T>
    {
        [Inject]
        public IStringLocalizer<Resources> L { get; set; } = default!;

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