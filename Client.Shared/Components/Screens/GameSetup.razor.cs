using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Client.Shared.Store;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class GameSetup
    {
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

        private MudForm? form;

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

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            this.form?.Validate();
        }

        private void SetupGame() => this.DispatchOnce<SubmitGameSetupAction>(new(
            new(this.PlayerCount, this.wordCount, this.teamCount, this.selectedRoundTypes.ToArray())));
    }
}