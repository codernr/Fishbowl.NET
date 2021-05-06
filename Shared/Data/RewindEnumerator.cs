using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class RewindEnumerator<T> : IReturnEnumerator<T>
    {
        public T Current => this.list[id];

        object IEnumerator.Current => this.Current!;

        private int id = -1;

        private readonly List<T> list;

        public RewindEnumerator(IEnumerable<T> collection) => this.list = new List<T>(collection);

        public void Dispose() {}

        public bool MoveNext()
        {
            if (this.id < this.list.Count - 1)
            {
                this.id++;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.id > -1)
            {
                this.id--;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.id = -1;
        }

        public void Return(T item)
        {
            if (this.id > -1)
            {
                this.id--;
            }
        }
    }
}