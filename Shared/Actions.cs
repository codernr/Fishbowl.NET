using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Shared.Actions
{
    public record ReceivePlayerCountAction(int TotalCount, int ConnectedCount, int SetupCount);

    public record ReceiveRestoreStateAction(PlayerViewModel Player, List<TeamViewModel> Teams);

    public record ReceiveWaitForTeamSetupAction(PlayerViewModel? SetupPlayer, List<TeamViewModel> Teams);
    
    public record ReceiveGameStartedAction();
    
    public record ReceiveGameAbortAction(string MessageKey);

    public record ReceiveTeamNameAction(int Id, string Name);

    public record ReceiveGameSetupAction(GameSetupViewModel Setup);
    
    public record ReceiveSetTeamNameAction(List<TeamViewModel> Teams);
    
    public record ReceiveWaitForOtherPlayersAction(string Username);
    
    public record ReceiveGameFinishedAction(GameSummaryViewModel Summary);
    
    public record ReceiveRoundStartedAction(string Type);
    
    public record ReceiveRoundFinishedAction(string Type, List<PeriodSummaryViewModel> Periods);
    
    public record ReceivePeriodSetupAction(PeriodSetupViewModel Setup);
    
    public record ReceivePeriodStartedAction(PeriodRunningViewModel Period);
        
    public record ReceivePeriodFinishedAction(PeriodSummaryViewModel Summary);
    
    public record ReceiveWordSetupAction(WordViewModel Word);

    public record ReceiveScoreAddedAction(ScoreViewModel Score);

    public record ReceiveLastScoreRevokedAction(ScoreViewModel Score);
    
    public record JoinGameContextAction(string Password, string Username);

    public record CreateGameContextAction(JoinGameContextAction GameContextJoin, GameSetupViewModel GameSetup);

    public record SubmitTeamNameAction(int Id, string Name);
    
    public record AddPlayerAction(string Username, IEnumerable<Word> Words);

    public record AddScoreAction(WordViewModel Word);

    public static class ActionExtensions
    {
        public static Player Map(this AddPlayerAction player) =>
            new(player.Username, player.Words);

        public static ReceiveSetTeamNameAction Map(this IEnumerable<Team> teams) =>
            new(teams.Select(team => team.Map()).ToList());

        public static ReceiveRoundFinishedAction MapSummary(this Round round) =>
            new(round.Type, round.Periods.Select(period => period.Map() as PeriodSummaryViewModel).ToList());
    }
}