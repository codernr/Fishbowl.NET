using System;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class State<T> : ComponentBase where T : State<T>
    {
        [Inject]
        public IStringLocalizer<Resources> L { get; set; } = default!;

        [Inject]
        public AppStateService AppState { get; set; } = default!;

        [Parameter]
        public Action<T>? SetParameters { get; set; }

        public string Title
        {
            get => this.title;
            set
            {
                this.title = value;
                this.AppState.Title = value;
            }
        }

        private string title = default!;

        public void Update(Action<T> updateParameters)
        {
            updateParameters(this.Instance);
            this.StateHasChanged();
        }

        private T Instance => this as T ?? throw new InvalidCastException();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Title = this.L[$"Components.States.{this.GetType().Name}.Title"];

            this.SetParameters?.Invoke(this.Instance);
        }
    }
}