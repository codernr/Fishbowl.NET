using System.Linq;
using Fishbowl.Net.Shared;
using Xunit;

namespace Fishbowl.Net.Tests.Shared
{
    public class SharedExtensionsTests
    {
        [Fact]
        public void TestDistribute()
        {
            var list = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };

            var list2 = list.Distribute(2).ToArray();

            Assert.Equal(2, list2.Length);
            Assert.Equal(new[] { "a", "c", "e", "g", "i" }, list2[0]);
            Assert.Equal(new[] { "b", "d", "f", "h", "j" }, list2[1]);

            var list3 = list.Distribute(3).ToArray();

            Assert.Equal(3, list3.Length);
            Assert.Equal(new[] { "a", "d", "g", "j" }, list3[0]);
            Assert.Equal(new[] { "b", "e", "h" }, list3[1]);
            Assert.Equal(new[] { "c", "f", "i" }, list3[1]);
        }
    }
}
