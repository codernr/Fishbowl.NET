using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Collections
{
    public class CircularEnumerator<T> : IEnumerator<T>
    {
        [JsonIgnore]
        public T Current => this.List[this.Id];

        object IEnumerator.Current => this.List[this.Id]!;

        public int Id { get; private set; } = 0;

        public IList<T> List { get; private set; }

        public CircularEnumerator(IList<T> list) => this.List = list;

        [JsonConstructor]
        public CircularEnumerator(int id, IList<T> list) : this(list) => this.Id = id;

        public void Dispose() { }

        public bool MoveNext()
        {
            this.Id = (this.Id + 1) % this.List.Count;
            return true;
        }

        public void Reset()
        {
            this.Id = 0;
        }
    }
}