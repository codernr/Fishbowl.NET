namespace Fishbowl.Net.Client.Shared.Common
{
    public partial class MainLayout
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.AppState.PropertyChanged += this.StateHasChanged;
            
            if (this.AppState.Title is null)
            {
                this.AppState.Title = this.L["Common.GameName"];
            }
        }
    }
}