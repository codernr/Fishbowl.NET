using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Store
{
    public record ReloadAction();

    public class CommonEffects
    {
        private readonly NavigationManager navigationManager;

        public CommonEffects(NavigationManager navigationManager) =>
            this.navigationManager = navigationManager;

        [EffectMethod(typeof(ReloadAction))]
        public Task Reload(IDispatcher dispatcher)
        {
            this.navigationManager.NavigateTo(this.navigationManager.Uri, true);
            return Task.CompletedTask;
        }
    }
}