using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.ViewModels;
using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class TeamName
    {
        protected override string Title => this.State.Value.Title;
        
        private TeamViewModel Team => this.State.Value.Team;

        private MudForm? form;

        private string teamName = string.Empty;

        private void Submit() =>
            this.Dispatcher.Dispatch(new SubmitTeamNameAction(this.Team.Id, this.teamName));
    }
}