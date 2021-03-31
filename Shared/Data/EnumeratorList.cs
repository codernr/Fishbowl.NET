using System.Collections;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class EnumeratorList<T> : List<T>, IEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public EnumeratorList() : base()
        {
            this.enumerator = this.GetEnumerator();
        }

        public EnumeratorList(IEnumerable<T> collection) : base(collection)
        {
            this.enumerator = this.GetEnumerator();
        }

        public EnumeratorList(int capacity) : base(capacity)
        {
            this.enumerator = this.GetEnumerator();
        }

        public T Current => this.enumerator.Current;

        object IEnumerator.Current => this.enumerator.Current!;

        public void Dispose() => this.enumerator.Dispose();

        public bool MoveNext() => this.enumerator.MoveNext();

        public void Reset() => this.enumerator.Reset();
    }
}