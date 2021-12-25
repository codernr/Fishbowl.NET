using Fishbowl.Net.Client.Pwa.Store;

namespace Fishbowl.Net.Client.Pwa.Pages
{
    public partial class Index
    {
        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
           
            this.Dispatcher.Dispatch(new InitGamePlayAction());
        }
    }
}