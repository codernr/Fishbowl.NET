using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class RoundFinished
    {
        public Round Round
        {
            get => this.round ?? throw new InvalidOperationException();
            set
            {
                this.round = value;
                this.StateHasChanged();
            }
        }

        private Round? round;
    }
}