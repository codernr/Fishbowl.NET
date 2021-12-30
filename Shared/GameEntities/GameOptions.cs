using System;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class GameOptions
    {
        [JsonIgnore]
        public TimeSpan PeriodLength => TimeSpan.FromSeconds(this.PeriodLengthInSeconds);

        public int PeriodLengthInSeconds { get; set; } = 60;

        [JsonIgnore]
        public TimeSpan PeriodThreshold => TimeSpan.FromSeconds(this.PeriodThresholdInSeconds);

        public int PeriodThresholdInSeconds { get; set; } = 5;
    }
}