using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Collections
{
    public interface IReturnEnumerator<T> : IEnumerator<T>
    {
        void Return(T item);
    }
}