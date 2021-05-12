using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Data.ViewModels
{
    public record PlayerCountViewModel(int SetupCount, int TotalCount);

    public record PlayerViewModel(Guid Id, string Name);

    public record TeamViewModel(int Id, IEnumerable<PlayerViewModel> Players);

    public record RoundViewModel(string Type);

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

    public static class ViewModelExtensions
    {
        public static PlayerViewModel Map(this Player player) => new(player.Id, player.Name);

        public static PeriodSetupViewModel Map(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.LengthInSeconds);

        public static PeriodRunningViewModel MapRunning(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.LengthInSeconds,
            period.StartedAt ?? throw new InvalidOperationException());

        public static PeriodSummaryViewModel Map(this Period period) =>
            new(period.Player.Map(), period.Scores.Select(score => score.Map()).ToList());

        public static WordViewModel Map(this Word word) => new(word.Id, word.Value);

        public static Word Map(this WordViewModel word) => new(word.Id, word.Value);

        public static ScoreViewModel Map(this Score score) => new(score.Word.Map(), score.Timestamp);

        public static Score Map(this ScoreViewModel score) => new(score.Word.Map(), score.Timestamp);
    }
}