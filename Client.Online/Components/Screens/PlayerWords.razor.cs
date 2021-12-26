using System;
using System.Linq;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.GameEntities;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PlayerWords
    {
        private MudForm? form;

        private string[] words = new string[0];

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.words = new string[this.State.Value.Setup.WordCount];
        }

        private void Submit() => this.DispatchOnce<AddPlayerAction>(new(
            this.State.Value.Username!, this.words.Select(word => new Word(Guid.NewGuid(), word))));
    }
}