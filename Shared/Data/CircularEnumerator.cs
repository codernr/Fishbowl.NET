using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class CircularEnumerator<T> : IEnumerator<T>
    {
        private readonly IList<T> list;

        private int id = 0;

        public CircularEnumerator(IList<T> list) => this.list = list;

        public T Current => this.list[this.id];

        object IEnumerator.Current => this.list[this.id]!;

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