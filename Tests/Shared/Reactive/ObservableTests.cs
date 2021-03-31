using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Reactive;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Reactive
{
    public class ObservableTests
    {
        [Fact]
        public async Task TestObservable()
        {
            var o = new Observable<int>(Counter());

            await Task.WhenAll(
                TestList(0, o),
                TestList(1, o),
                TestList(2, o)
            );
        }

        private static async Task TestList(int id, IAsyncEnumerable<int> input)
        {
            List<int> numbers = new();

            await foreach (var item in input)
            {
                numbers.Add(item);
            }

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, numbers);
        }

        private static async IAsyncEnumerable<int> Counter()
        {
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(20);
                yield return i;
            }
        }
    }
}