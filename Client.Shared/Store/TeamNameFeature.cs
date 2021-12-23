using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TeamNameState(TeamViewModel? Team);

    public record SetTeamNameAction(TeamViewModel Team);

    public record SubmitTeamNameAction(string TeamName);

    public static class TeamNameReducers
    {
        [ReducerMethod]
        public static TeamNameState OnTeamNameSet(TeamNameState state, SetTeamNameAction action) =>
            new(action.Team);
    }
}