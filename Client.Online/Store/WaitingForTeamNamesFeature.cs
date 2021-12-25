using System.Collections.Generic;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record WaitingForTeamNamesState
    {
        public PlayerViewModel? SetupPlayer { get; init; }

        public TeamViewModel Team { get; init; } = default!;

        public ICollection<TeamViewModel> Teams { get; init; } = default!;
    }

    public record SetWaitingForTeamNamesAction(PlayerViewModel? SetupPlayer, TeamViewModel Team);

    public record SetWaitingForTeamNamesTeamAction(ICollection<TeamViewModel> Teams);

    public static class WaitingForTeamNamesReducers
    {
        [ReducerMethod]
        public static WaitingForTeamNamesState OnSetWaitingForTeamNames(
            WaitingForTeamNamesState state, SetWaitingForTeamNamesAction action) =>
            state with { SetupPlayer = action.SetupPlayer, Team = action.Team };

        [ReducerMethod]
        public static WaitingForTeamNamesState OnSetWaitingForTeamNamesTeam(
            WaitingForTeamNamesState state, SetWaitingForTeamNamesTeamAction action) =>
            state with { Teams = action.Teams };
    }
}