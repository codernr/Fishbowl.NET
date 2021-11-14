using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.Collections
{
    public class ShuffleEnumerator<T> : IShuffleEnumerator<T>
    {
        public T Current => this.id > -1 ? this.nextItems.Peek() : throw new InvalidOperationException("Invalid state.");

        object IEnumerator.Current => this.Current ??
            throw new InvalidOperationException("Invalid state.");

        private Stack<T> previousItems = new();

        private Stack<T> nextItems;

        private int id = -1;

        public ShuffleEnumerator(IEnumerable<T> collection) =>
            this.nextItems = new(collection);

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            if (this.id < 0)
            {
                this.id++;
                return true;
            }

            if (this.id < this.nextItems.Count + this.previousItems.Count - 1)
            {
                this.id++;
                this.previousItems.Push(this.nextItems.Pop());
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.id == 0)
            {
                this.id--;
                return true;
            }
            
            if (this.id > 0)
            {
                this.id--;
                this.nextItems.Push(this.previousItems.Pop());
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.nextItems = new Stack<T>(this.nextItems.Concat(this.previousItems).Randomize());
            this.previousItems.Clear();
        }

        public void Shuffle() => this.nextItems.Randomize();
    }
}