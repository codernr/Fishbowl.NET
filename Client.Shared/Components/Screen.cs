using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Store;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class Screen : FluxorComponent
    {
        [Inject]
        public IStringLocalizer<Resources> L { get; set; } = default!;

        [Inject]
        public IDispatcher Dispatcher { get; set; } = default!;

        protected virtual string Title => this.L[$"Components.States.{this.GetType().Name}.Title"];

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Dispatcher.Dispatch(new SetAppBarTitleAction(this.Title));
        }
    }

}