using System;
using System.Collections;
using System.Collections.Generic;
using Fishbowl.Net.Shared.Exceptions;

namespace Fishbowl.Net.Shared.Collections
{
    public class RandomEnumerator<T> : IReturnEnumerator<T>
    {
        public T Current { get; private set; } = default!;

        object IEnumerator.Current { get => this.Current ??
            throw new InvalidOperationException("Invalid state."); }

        public List<T> List { get; private set; }

        public Stack<T> Stack { get; private set; }

        public RandomEnumerator(IEnumerable<T> collection) =>
            (this.List, this.Stack) = (new List<T>(collection), new Stack<T>(collection.Randomize()));

        public RandomEnumerator(T current, Stack<T> stack, List<T> list) : this(list) =>
            (this.Current, this.Stack) = (current, stack);

        public void Dispose() {}

        public bool MoveNext()
        {
            if (this.Stack.Count > 0)
            {
                this.Current = this.Stack.Pop();
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.Stack.Count < this.List.Count)
            {
                this.Stack.Push(this.Current);
                this.Stack = new Stack<T>(this.Stack.Randomize());
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.Stack = new Stack<T>(this.List.Randomize());
        }

        public void Return(T item)
        {
            if (this.Stack.Contains(item) || !this.List.Contains(item))
            {
                throw new InvalidReturnValueException();
            }

            this.Stack.Push(item);
            this.Stack = new Stack<T>(this.Stack.Randomize());
        }
    }
}