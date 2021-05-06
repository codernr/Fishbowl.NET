using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public interface IReturnEnumerator<T> : IEnumerator<T>
    {
        void Return(T item);
    }
}