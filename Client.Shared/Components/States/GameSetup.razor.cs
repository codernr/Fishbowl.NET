using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class GameSetup
    {
        public Func<GameSetupViewModel, Task> OnGameSetup { get; set; } = default!;

        private bool IsValid => this.SelectedRoundTypes.Count() > 0;

        private int PlayerCount
        {
            get => this.playerCount;
            set
            {
                this.playerCount = value;
                this.teamCount = 2;
                this.StateHasChanged();
            }
        }

        private int MaxTeamCount => this.PlayerCount / 2;

        private IEnumerable<string> SelectedRoundTypes
        {
            get => this.selectedRoundTypes;
            set
            {
                this.selectedRoundTypes = value;
                this.StateHasChanged();
            }
        }

        private int playerCount = 4;

        private int teamCount = 2;

        private int wordCount = 1;

        private IEnumerable<string> roundTypes = default!;

        private IEnumerable<string> selectedRoundTypes = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.roundTypes = new[] { "Taboo", "Drawing", "Charades", "Password", "Humming" }
                .Select(item => L[$"Components.States.GameSetup.RoundTypes.{item}"].Value);
            this.SelectedRoundTypes = this.roundTypes;
        }

        private Task SetupGame() =>
            this.Submit(() => this.OnGameSetup(
                new(this.PlayerCount, this.wordCount, this.teamCount, this.selectedRoundTypes.ToArray())));
    }
}