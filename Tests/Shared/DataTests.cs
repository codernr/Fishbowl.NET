using System.Text.Json;
using Fishbowl.Net.Shared.Collections;
using Fishbowl.Net.Shared.Data;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Data
{
    public class DataTests
    {
        [Fact]
        public void RandomEnumeratorSerializationTest()
        {
            var list = new TestData[]
            {
                new("d1"), new("d2"), new("d3"), new("d4"), new("d5") 
            };

            var original = new RandomEnumerator<TestData>(list);

            var mapped = original.Map();

            var json = JsonSerializer.Serialize(mapped);

            var deserialized = JsonSerializer.Deserialize<RandomEnumeratorData<TestData>>(json);

            var restored = deserialized?.Map();
        }

        private record TestData(string Id);
    }
}