using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TeamNameState(TeamViewModel? Team = null, string Title = "");

    public record SetTeamNameAction(TeamViewModel Team, string Title);

    public record SubmitTeamNameAction(string TeamName);

    public static class TeamNameReducers
    {
        [ReducerMethod]
        public static TeamNameState OnTeamNameSet(TeamNameState state, SetTeamNameAction action) =>
            new(action.Team, action.Title);
    }
}