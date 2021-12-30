using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Collections
{
    public interface IRewindList<T>
    {
        T Current { get; }
        
        Stack<T> PreviousItems { get; }

        Stack<T> NextItems { get; }

        bool Rewound { get; }

        bool MoveNext();

        bool MovePrevious();
    }
}