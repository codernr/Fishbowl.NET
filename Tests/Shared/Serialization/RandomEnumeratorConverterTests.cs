using System;
using System.Linq;
using System.Text.Json;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.Serialization;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Serialization
{
    public class RandomEnumeratorConverterTests
    {
        [Fact]
        public void ShouldSerializeCorrectly()
        {
            var list = new Word[]
            {
                new(Guid.NewGuid(), "w1"),
                new(Guid.NewGuid(), "w2"),
                new(Guid.NewGuid(), "w3"),
                new(Guid.NewGuid(), "w4"),
                new(Guid.NewGuid(), "w5") 
            };

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new RandomEnumeratorConverter());

            var original = new RandomEnumerator<Word>(list);

            var json1 = JsonSerializer.Serialize(original, options);
            
            original.MoveNext();
            original.MoveNext();

            var json2 = JsonSerializer.Serialize(original, options);

            var deserialized1 = JsonSerializer.Deserialize<RandomEnumerator<Word>>(json1, options);
            var deserialized2 = JsonSerializer.Deserialize<RandomEnumerator<Word>>(json2, options);

            Assert.True(deserialized1!.Stack.All(item => deserialized1.List.Contains(item)));
            Assert.Null(deserialized1.Current);
            Assert.Equal(original.Current, deserialized2!.Current);
            Assert.Equal(3, deserialized2.Stack.Count);
        }
    }
}