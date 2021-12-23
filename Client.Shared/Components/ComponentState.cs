using System;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Services;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class ComponentState<T> : ComponentBase, IDisposable where T : class
    {
        [Inject]
        public IStringLocalizer<Resources> L { get; set; } = default!;

        [Inject]
        public AppStateService AppState { get; set; } = default!;

        [Inject]
        public IState<T> State { get; set; } = default!;

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

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Title = this.L[$"Components.States.{this.GetType().Name}.Title"];

            this.State.StateChanged += this.StateChanged;
        }

        protected virtual void StateChanged(object? sender, T state) => this.InvokeAsync(this.StateHasChanged);

        public void Dispose() => this.State.StateChanged -= this.StateChanged;
    }
}