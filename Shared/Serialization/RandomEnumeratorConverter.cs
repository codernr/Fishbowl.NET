using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.Serialization
{
    public class RandomEnumeratorConverter : JsonConverter<RandomEnumerator<Word>>
    {
        public override RandomEnumerator<Word>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Map(JsonSerializer.Deserialize<Data>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, RandomEnumerator<Word> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, Map(value), options);

        private record Data(Word? Current, IEnumerable<Word> List, IEnumerable<Word> Stack);

        private static Data Map(RandomEnumerator<Word> enumerator) =>
            new(enumerator.Current, enumerator.List, enumerator.Stack);

        private static RandomEnumerator<Word>? Map(Data? data)
        {
            if (data is null)
            {
                return null;
            }

            if (data.Current is null)
            {
                return new(data.List);
            }

            var current = data.List.First(item => item == data.Current);
            var stack = new Stack<Word>(data.Stack.Select(item => data.List.First(listItem => listItem == item)));

            return new(current, stack, data.List.ToList());
        }
    }
}