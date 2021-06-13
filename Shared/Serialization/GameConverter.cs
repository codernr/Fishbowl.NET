using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.Serialization
{
    public class GameConverter : JsonConverter<Game>
    {
        public override Game? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Map(JsonSerializer.Deserialize<Data>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, Game value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, Map(value));

        private record Data(
            Guid Id,
            CircularEnumerator<Team> TeamEnumerator,
            List<Round> Rounds,
            int CurrentRoundIndex,
            double PeriodLengthInSeconds,
            double PeriodThresholdInSeconds);

        private static Data Map(Game game) => new(
            game.Id,
            game.TeamEnumerator,
            game.Rounds,
            game.Rounds.IndexOf(game.CurrentRound),
            game.periodLength.TotalSeconds,
            game.periodThreshold.TotalSeconds);

        private static Game? Map(Data? data)
        {
            if (data is null) return null;

            var roundEnumerator = data.Rounds.GetEnumerator();

            for (int i = 0; i < data.CurrentRoundIndex + 1; i++) roundEnumerator.MoveNext();

            return new(
                data.Id,
                data.TeamEnumerator,
                data.Rounds,
                roundEnumerator,
                TimeSpan.FromSeconds(data.PeriodLengthInSeconds),
                TimeSpan.FromSeconds(data.PeriodThresholdInSeconds));
        }
    }
}