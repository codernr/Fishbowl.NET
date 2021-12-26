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

    public record ReceiveGameSetupAction(int PlayerCount, int WordCount, int TeamCount, string[] RoundTypes) :
        GameSetupViewModel(PlayerCount, WordCount, TeamCount, RoundTypes);
    
    public record ReceiveSetTeamNameAction(List<TeamViewModel> Teams);
    
    public record ReceiveWaitForOtherPlayersAction(string Username);
    
    public record ReceiveGameFinishedAction(List<TeamSummaryViewModel> Teams) : GameSummaryViewModel(Teams);
    
    public record ReceiveRoundStartedAction(string Type);
    
    public record ReceiveRoundFinishedAction(string Type, List<PeriodSummaryViewModel> Periods);
    
    public record ReceivePeriodSetupAction(RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds) :
        PeriodSetupViewModel(Round, Player, LengthInSeconds);
    
    public record ReceivePeriodStartedAction(
        RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds, DateTimeOffset StartedAt, int ScoreCount) :
        PeriodRunningViewModel(Round, Player, LengthInSeconds, StartedAt, ScoreCount);
        
    public record ReceivePeriodFinishedAction(PlayerViewModel Player, List<ScoreViewModel> Scores) :
        PeriodSummaryViewModel(Player, Scores);
    
    public record ReceiveWordSetupAction(Guid Id, string Value) : WordViewModel(Id, Value);

    public record ReceiveScoreAddedAction(WordViewModel Word, DateTimeOffset Timestamp) :
        ScoreViewModel(Word, Timestamp);

    public record ReceiveLastScoreRevokedAction(WordViewModel Word, DateTimeOffset Timestamp) :
        ReceiveScoreAddedAction(Word, Timestamp);
    
    public record JoinGameContextAction(string Password, string Username);

    public record CreateGameContextAction(JoinGameContextAction GameContextJoin, GameSetupViewModel GameSetup);

    public record SubmitTeamNameAction(int Id, string Name);
    
    public record AddPlayerAction(string Username, IEnumerable<Word> Words);

    public record AddScoreAction(WordViewModel Word);

    public static class ActionExtensions
    {
        public static Player Map(this AddPlayerAction player) =>
            new(player.Username, player.Words);

        public static ReceiveGameSetupAction Map(this GameSetupViewModel model) =>
            new(model.PlayerCount, model.WordCount, model.TeamCount, model.RoundTypes);

        public static ReceiveSetTeamNameAction Map(this IEnumerable<Team> teams) =>
            new(teams.Select(team => team.Map()).ToList());

        public static ReceiveRoundFinishedAction MapSummary(this Round round) =>
            new(round.Type, round.Periods.Select(period => period.Map() as PeriodSummaryViewModel).ToList());

        public static ReceivePeriodSetupAction Map(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Username), period.Length.TotalSeconds);

        public static ReceivePeriodStartedAction MapRunning(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Username), period.Length.TotalSeconds,
            period.StartedAt ?? throw new InvalidOperationException(), period.Scores.Count);

        public static ReceivePeriodFinishedAction Map(this Period period) =>
            new(period.Player.Map(), period.Scores.Select(score => score.Map() as ScoreViewModel).ToList());

        public static ReceiveWordSetupAction Map(this Word word) => new(word.Id, word.Value);

        public static ReceiveLastScoreRevokedAction Map(this Score score) =>
            new(score.Word.Map() as WordViewModel, score.Timestamp);

        public static ReceiveGameFinishedAction Map(this Game game) => new(game.Teams
            .Select(team => team.Map(game))
            .OrderByDescending(team => team.TotalScoreCount)
            .ThenBy(team => team.TotalTime)
            .ToList());
    }
}