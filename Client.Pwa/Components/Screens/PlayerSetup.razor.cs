using Fishbowl.Net.Client.Pwa.Store;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Components.Screens
{
    public partial class PlayerSetup
    {
        protected override string Title => string.Format(base.Title, this.State.Value.PlayerIndex + 1);
        
        private MudForm? form;

        private string playerName = string.Empty;

        private string[] words = new string[2];

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.words = new string[this.State.Value.WordCount];
        }

        private void Submit() => this.DispatchOnce<SubmitPlayerSetupAction>(new(this.playerName, this.words));
    }
}