using System;
using System.Linq;
using System.Text.Json;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.Serialization;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Serialization
{
    public class ShuffleEnumeratorConverterTests
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
            options.Converters.Add(new ShuffleEnumeratorConverter());

            var original = new ShuffleEnumerator<Word>(list);

            var json1 = JsonSerializer.Serialize(original, options);
            
            original.MoveNext();
            original.MoveNext();

            var json2 = JsonSerializer.Serialize(original, options);

            var deserialized1 = JsonSerializer.Deserialize<ShuffleEnumerator<Word>>(json1, options);
            var deserialized2 = JsonSerializer.Deserialize<ShuffleEnumerator<Word>>(json2, options);

            Assert.Throws<InvalidOperationException>(() => deserialized1!.Current);
            Assert.Equal(0, deserialized1!.PreviousItems.Count);
            Assert.Equal(5, deserialized1!.NextItems.Count);
            Assert.Equal("w1", deserialized2!.Current.Value);
            Assert.Equal(1, deserialized2!.PreviousItems.Count);
            Assert.Equal(4, deserialized2!.NextItems.Count);
        }
    }
}