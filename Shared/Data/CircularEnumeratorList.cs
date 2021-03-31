using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class CircularEnumeratorList<T> : List<T>, IEnumerator<T>
    {
        private int id = 0;

        public CircularEnumeratorList() : base() {}

        public CircularEnumeratorList(IEnumerable<T> collection) : base(collection) {}

        public CircularEnumeratorList(int capacity) : base(capacity) {}

        public T Current => this[this.id];

        object IEnumerator.Current => this[this.id]!;

        public void Dispose() { }

        public bool MoveNext()
        {
            this.id = (this.id + 1) % this.Count;
            return true;
        }

        public void Reset()
        {
            this.id = 0;
        }
    }
}