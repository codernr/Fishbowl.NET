using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.ViewModels
{
    public record StatusResponse
    {
        public StatusResponse() {}

        public StatusResponse(StatusCode status) => this.Status = status;

        public StatusCode Status { get; init; } = StatusCode.Ok;
    }

    public record StatusResponse<T> : StatusResponse where T : notnull
    {
        public T Data
        {
            get => this.data ?? throw new InvalidOperationException();
            init => this.data = value;
        }

        private readonly T? data;

        public StatusResponse() : base() {}

        public StatusResponse(StatusCode status) : base(status) {}

        public StatusResponse(StatusCode status, T data) : base(status) =>
            this.data = data;
    }

    public record PlayerViewModel(string Username);

    public record TeamViewModel(int Id, List<PlayerViewModel> Players, string? Name = null);

    public record TeamSetupViewModel(List<TeamViewModel> Teams);

    public record RoundViewModel(string Type);

    public record RoundSummaryViewModel(string Type, List<PeriodSummaryViewModel> Periods);

    public record PlayerSummaryViewModel(string Username, List<ScoreSummaryViewModel> Scores);

    public record TeamSummaryViewModel(int Id, string Name, List<PlayerSummaryViewModel> Players, int TotalScoreCount, TimeSpan TotalTime);

    public record GameSummaryViewModel(List<TeamSummaryViewModel> Teams);

    public record PeriodSetupViewModel(RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds)
    {
        [JsonIgnore]
        public TimeSpan Length => TimeSpan.FromSeconds(this.LengthInSeconds);
    }

    public record PeriodRunningViewModel(
        RoundViewModel Round, PlayerViewModel Player, double LengthInSeconds, DateTimeOffset StartedAt, int ScoreCount) :
        PeriodSetupViewModel(Round, Player, LengthInSeconds);

    public record PeriodSummaryViewModel(PlayerViewModel Player, List<ScoreViewModel> Scores);

    public record WordViewModel(Guid Id, string Value);

    public record ScoreViewModel(WordViewModel Word, DateTimeOffset Timestamp);

    public record ScoreSummaryViewModel(WordViewModel Word, TimeSpan GuessedTime);

    public record GameSetupViewModel(int PlayerCount, int WordCount, int TeamCount, string[] RoundTypes);
    
    public static class ViewModelExtensions
    {
        public static PlayerViewModel Map(this Player player) => new(player.Username);

        public static TeamViewModel Map(this Team team) => new(team.Id, team.Players
            .Select(player => player.Map()).ToList(), team.Name);

        public static RoundViewModel Map(this Round round) => new(round.Type);

        public static PeriodSetupViewModel Map(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Username), period.Length.TotalSeconds);

        public static PeriodRunningViewModel MapRunning(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Username), period.Length.TotalSeconds,
            period.StartedAt ?? throw new InvalidOperationException(), period.Scores.Count);

        public static PeriodSummaryViewModel Map(this Period period) =>
            new(period.Player.Map(), period.Scores.Select(score => score.Map()).ToList());

        public static WordViewModel Map(this Word word) => new(word.Id, word.Value);

        public static Word Map(this WordViewModel word) => new(word.Id, word.Value);

        public static ScoreViewModel Map(this Score score) => new(score.Word.Map(), score.Timestamp);

        public static ScoreSummaryViewModel Map(this Score score, DateTimeOffset previous) =>
            new(score.Word.Map(), score.Timestamp - previous);

        public static Score Map(this ScoreViewModel score) => new(score.Word.Map(), score.Timestamp);

        public static RoundSummaryViewModel MapSummary(this Round round) =>
            new(round.Type, round.Periods.Select(period => period.Map()).ToList());

        public static PlayerSummaryViewModel Map(this Player player, Game game) =>
            new(player.Username, game.Rounds
                .SelectMany(round => round.Periods)
                .Where(period => period.Player.Username == player.Username)
                .SelectMany(period => period.MapScores())
                .ToList());

        public static IEnumerable<ScoreSummaryViewModel> MapScores(this Period period) =>
            period.Scores.Select((score, i) => score.Map((i > 0 ? period.Scores[i - 1].Timestamp : period.StartedAt!.Value)));

        public static TeamSummaryViewModel Map(this Team team, Game game)
        {
            var players = team.Players.Select(player => player.Map(game)).ToList();
            return new(team.Id, team.Name!, players, 
                players.Sum(player => player.Scores.Count()),
                new TimeSpan(players.Sum(player => player.Scores.Sum(score => score.GuessedTime.Ticks))));
        }

        public static TeamSetupViewModel Map(this IEnumerable<Team> teams) =>
            new(teams.Select(team => team.Map()).ToList());

        public static GameSummaryViewModel Map(this Game game) => new(game.Teams
            .Select(team => team.Map(game))
            .OrderByDescending(team => team.TotalScoreCount)
            .ThenBy(team => team.TotalTime)
            .ToList());
    }
}