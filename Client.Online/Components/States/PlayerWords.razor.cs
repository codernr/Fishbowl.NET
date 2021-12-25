using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Store;
using Fishbowl.Net.Client.Shared.Common;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PlayerWords
    {
        private MudForm? form;

        private string[] words = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.words = new string[this.State.Value.WordCount];
        }

        private void Submit() => this.Dispatcher.Dispatch(new SubmitPlayerWordsAction(this.words));
    }
}