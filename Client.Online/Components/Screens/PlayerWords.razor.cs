using System;
using System.Linq;
using Fishbowl.Net.Client.Online.Store;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.GameEntities;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.Screens
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

        private void Submit() => this.Dispatcher.Dispatch(new AddPlayerAction(
            this.State.Value.Username!, this.words.Select(word => new Word(Guid.NewGuid(), word))));
    }
}