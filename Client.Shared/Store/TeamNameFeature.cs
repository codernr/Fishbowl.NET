using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Shared.Store
{
    [FeatureState]
    public record TeamNameState
    {
        public TeamViewModel Team  { get; init; } = default!;
        public string Title { get; init; } = string.Empty;
    }

    public record SetTeamNameAction(TeamViewModel Team, string Title);

    public static class TeamNameReducers
    {
        [ReducerMethod]
        public static TeamNameState OnTeamNameSet(TeamNameState state, SetTeamNameAction action) =>
            state with { Team = action.Team, Title = action.Title };
    }
}