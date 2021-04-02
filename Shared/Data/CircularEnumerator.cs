using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class CircularEnumerator<T> : IEnumerator<T>
    {
        public T Current => this.list[this.id];

        object IEnumerator.Current => this.list[this.id]!;

        private int id = 0;

        private readonly IList<T> list;

        public CircularEnumerator(IList<T> collection) => this.list = collection;

        public void Dispose() { }

        public bool MoveNext()
        {
            this.id = (this.id + 1) % this.list.Count;
            return true;
        }

        public void Reset()
        {
            this.id = 0;
        }
    }
}