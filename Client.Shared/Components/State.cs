using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class State : ComponentBase
    {
        public void Update() => this.StateHasChanged();
    }
}