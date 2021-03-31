using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public interface IRewindEnumerator<T> : IEnumerator<T>
    {
        bool MovePrevious();
    }
}