using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class CircularEnumerator<T> : IEnumerator<T>
    {
        public T Current => this.list[this.id];

        object IEnumerator.Current => this.list[this.id]!;

        private int id = 0;

        private readonly List<T> list;

        public CircularEnumerator(IEnumerable<T> collection) => this.list = new List<T>(collection);

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