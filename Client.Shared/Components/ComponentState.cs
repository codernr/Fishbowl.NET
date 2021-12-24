using System;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Services;
using Fishbowl.Net.Client.Shared.Store;
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
        public IState<T> State { get; set; } = default!;

        [Inject]
        public IDispatcher Dispatcher { get; set; } = default!;

        protected virtual string Title => this.L[$"Components.States.{this.GetType().Name}.Title"];

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Dispatcher.Dispatch(new SetAppBarTitleAction(this.Title));

            this.State.StateChanged += this.StateChanged;
        }

        protected virtual void StateChanged(object? sender, T state) => this.InvokeAsync(this.StateHasChanged);

        public void Dispose() => this.State.StateChanged -= this.StateChanged;
    }
}