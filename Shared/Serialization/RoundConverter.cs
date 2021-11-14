using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.Serialization
{
    public class RoundConverter : JsonConverter<Round>
    {
        public override Round? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Map(JsonSerializer.Deserialize<Data>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, Round value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, Map(value), options);

        private record Data(string Type, ShuffleEnumerator<Word> WordEnumerator, List<Period> Periods);

        private static Data Map(Round round) =>
            new(round.Type, round.WordEnumerator as ShuffleEnumerator<Word> ?? throw new InvalidOperationException(), round.Periods);

        private static Round? Map(Data? data) => data is null ? null :
            new(data.Type, data.WordEnumerator, data.Periods);
    }
}