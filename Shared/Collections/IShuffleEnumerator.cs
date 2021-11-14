using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Collections
{
    public interface IShuffleEnumerator<T> : IEnumerator<T>
    {
        bool MovePrevious();

        void Shuffle();
    }
}