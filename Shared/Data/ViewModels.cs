using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Data.ViewModels
{
    public record PlayerCountViewModel(int TotalCount, int ConnectedCount, int SetupCount);

    public record PlayerViewModel(Guid Id, string Name);

    public record TeamViewModel(int Id, List<PlayerViewModel> Players, string? Name = null);

    public record TeamSetupViewModel(List<TeamViewModel> Teams);

    public record TeamNameViewModel(int Id, string Name);

    public record RoundViewModel(string Type);

    public record RoundSummaryViewModel(string Type, List<PeriodSummaryViewModel> Periods);

    public record PlayerSummaryViewModel(Guid Id, string Name, List<ScoreViewModel> Scores);

    public record TeamSummaryViewModel(int Id, string Name, List<PlayerSummaryViewModel> Players);

    public record GameSummaryViewModel(List<TeamSummaryViewModel> Teams);

    public record PeriodSetupViewModel(RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds)
    {
        [JsonIgnore]
        public TimeSpan Length => TimeSpan.FromSeconds(this.LengthInSeconds);
    }

    public record PeriodRunningViewModel(
        RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds, DateTimeOffset StartedAt) :
        PeriodSetupViewModel(Round, Player, LengthInSeconds);

    public record PeriodSummaryViewModel(PlayerViewModel Player, List<ScoreViewModel> Scores);

    public record WordViewModel(Guid Id, string Value);

    public record ScoreViewModel(WordViewModel Word, DateTimeOffset Timestamp);

    public record GameAbortViewModel(string MessageKey);

    public record GameSetupViewModel(int PlayerCount, int WordCount, int TeamCount, string[] RoundTypes);
    
    public record GameContextJoinViewModel(string Password, Guid UserId);

    public record GameContextSetupViewModel(GameContextJoinViewModel GameContextJoin, GameSetupViewModel GameSetup);

    public static class ViewModelExtensions
    {
        public static PlayerViewModel Map(this Player player) => new(player.Id, player.Name);

        public static TeamViewModel Map(this Team team) => new(team.Id, team.Players
            .Select(player => player.Map()).ToList(), team.Name);

        public static RoundViewModel Map(this Round round) => new(round.Type);

        public static PeriodSetupViewModel Map(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.Length.TotalSeconds);

        public static PeriodRunningViewModel MapRunning(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.Length.TotalSeconds,
            period.StartedAt ?? throw new InvalidOperationException());

        public static PeriodSummaryViewModel Map(this Period period) =>
            new(period.Player.Map(), period.Scores.Select(score => score.Map()).ToList());

        public static WordViewModel Map(this Word word) => new(word.Id, word.Value);

        public static Word Map(this WordViewModel word) => new(word.Id, word.Value);

        public static ScoreViewModel Map(this Score score) => new(score.Word.Map(), score.Timestamp);

        public static Score Map(this ScoreViewModel score) => new(score.Word.Map(), score.Timestamp);

        public static RoundSummaryViewModel MapSummary(this Round round) =>
            new(round.Type, round.Periods.Select(period => period.Map()).ToList());

        public static PlayerSummaryViewModel Map(this Player player, Game game) =>
            new(player.Id, player.Name, game.Rounds
                .SelectMany(round => round.Periods)
                .Where(period => period.Player.Id == player.Id)
                .SelectMany(period => period.Scores.Select(score => score.Map()))
                .ToList());

        public static TeamSummaryViewModel Map(this Team team, Game game) =>
            new(team.Id, team.Name ?? throw new InvalidOperationException(),
                team.Players.Select(player => player.Map(game)).ToList());

        public static TeamSetupViewModel Map(this IEnumerable<Team> teams) =>
            new(teams.Select(team => team.Map()).ToList());

        public static GameSummaryViewModel Map(this Game game) => new(game.Teams
            .Select(team => team.Map(game))
            .OrderByDescending(team => team.Players.Sum(player => player.Scores.Count))
            .ToList());
    }
}