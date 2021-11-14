using Fishbowl.Net.Shared.Collections;
using Xunit;

namespace Fishbowl.Net.Tests.Collections
{
    public class ShuffleEnumeratorTests
    {
        [Fact]
        public void ShouldReturnFalseWhenFinished()
        {
            var enumerator = new ShuffleEnumerator<string>(new[] { "d", "c", "b", "a" });

            Assert.True(enumerator.MoveNext());
            Assert.Equal("a", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("c", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("d", enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void ShouldMovePrevious()
        {
            var enumerator = new ShuffleEnumerator<string>(new[] { "d", "c", "b", "a" });

            Assert.True(enumerator.MoveNext());
            Assert.Equal("a", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("c", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("d", enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.True(enumerator.MovePrevious());
            Assert.Equal("c", enumerator.Current);
            Assert.True(enumerator.MovePrevious());
            Assert.Equal("b", enumerator.Current);
            Assert.True(enumerator.MovePrevious());
            Assert.Equal("a", enumerator.Current);
            Assert.False(enumerator.MovePrevious());
            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current);
            Assert.True(enumerator.MovePrevious());
            Assert.Equal("a", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.True(enumerator.MoveNext());
            Assert.True(enumerator.MovePrevious());
            Assert.Equal("c", enumerator.Current);
        }
    }
}