using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Collections
{
    public interface IGameList<T>
    {
        int CurrentId { get; }

        IList<T> List { get; }

        T Current { get; }

        bool MoveNext();
    }
}