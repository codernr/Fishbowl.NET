using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.Collections
{
    public class ShuffleEnumerator<T> : IShuffleEnumerator<T>
    {
        public T Current => this.Id > -1 ? this.NextItems.Peek() : throw new InvalidOperationException("Invalid state.");

        object IEnumerator.Current => this.Current ??
            throw new InvalidOperationException("Invalid state.");

        public Stack<T> PreviousItems { get; private set; } = new();

        public Stack<T> NextItems { get; private set; }

        public int Id { get; private set; } = -1;

        public ShuffleEnumerator(IEnumerable<T> collection) =>
            this.NextItems = new(collection);

        public ShuffleEnumerator(int id, IEnumerable<T> nextItems, IEnumerable<T> previousItems) =>
            (this.Id, this.NextItems, this.PreviousItems) = (id, new(nextItems), new(previousItems));

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            if (this.Id < 0)
            {
                this.Id++;
                return true;
            }

            if (this.Id < this.NextItems.Count + this.PreviousItems.Count - 1)
            {
                this.Id++;
                this.PreviousItems.Push(this.NextItems.Pop());
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.Id == 0)
            {
                this.Id--;
                return true;
            }
            
            if (this.Id > 0)
            {
                this.Id--;
                this.NextItems.Push(this.PreviousItems.Pop());
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.NextItems = new Stack<T>(this.NextItems.Concat(this.PreviousItems).Randomize());
            this.PreviousItems.Clear();
        }

        public void Shuffle() => this.NextItems.Randomize();
    }
}