using System;
using System.Collections.Generic;
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

    public record PeriodSummaryViewModel(PlayerViewModel Player, List<string> Scores);

    public static class ViewModelExtensions
    {
        public static PeriodSetupViewModel Map(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.LengthInSeconds);

        public static PeriodRunningViewModel MapRunning(this Period period, Round round) =>
            new(new(round.Type), new(period.Player.Id, period.Player.Name), period.LengthInSeconds,
            period.StartedAt ?? throw new InvalidOperationException());
    }
}