using System;
using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class RandomEnumerator<T> : IReturnEnumerator<T>
    {
        public T Current { get; private set; } = default!;

        object IEnumerator.Current { get => this.Current ??
            throw new InvalidOperationException("Invalid state."); }

        private readonly List<T> list;

        private Stack<T> stack;

        public RandomEnumerator(IEnumerable<T> collection) =>
            (this.list, this.stack) = (new List<T>(collection), new Stack<T>(collection.Randomize()));

        public void Dispose() {}

        public bool MoveNext()
        {
            if (this.stack.Count > 0)
            {
                this.Current = this.stack.Pop();
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.stack.Count < this.list.Count)
            {
                this.stack.Push(this.Current);
                this.stack = new Stack<T>(this.stack.Randomize());
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.stack = new Stack<T>(this.list.Randomize());
        }

        public void Return(T item)
        {
            if (this.stack.Contains(item) || !this.list.Contains(item))
            {
                throw new ArgumentException();
            }

            this.stack.Push(item);
            this.stack = new Stack<T>(this.stack.Randomize());
        }
    }
}