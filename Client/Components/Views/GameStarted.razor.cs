using System;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class GameStarted
    {
        public Team Team
        { 
            get => this.team ?? throw new InvalidOperationException();
            set
            {
                this.team = value;
                this.StateHasChanged();
            }
        }

        private Team? team;
    }
}