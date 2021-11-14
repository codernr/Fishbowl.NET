using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.Serialization
{
    public class ShuffleEnumeratorConverter : JsonConverter<ShuffleEnumerator<Word>>
    {
        public override ShuffleEnumerator<Word>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Map(JsonSerializer.Deserialize<Data>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, ShuffleEnumerator<Word> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, Map(value), options);

        private record Data(int Id, IEnumerable<Word> NextItems, IEnumerable<Word> PreviousItems);

        private static Data Map(ShuffleEnumerator<Word> enumerator) =>
            new(enumerator.Id, enumerator.NextItems, enumerator.PreviousItems);

        private static ShuffleEnumerator<Word>? Map(Data? data)
        {
            if (data is null)
            {
                return null;
            }

            return new(data.Id, data.NextItems, data.PreviousItems);
        }
    }
}