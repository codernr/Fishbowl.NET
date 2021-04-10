using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class GameFinished
    {
        public Game Game
        {
            get => this.game ?? throw new InvalidOperationException();
            set
            {
                this.game = value;
                this.StateHasChanged();
            }
        }

        private Game? game;
    }
}