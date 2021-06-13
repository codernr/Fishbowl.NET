using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Collections
{
    public class RewindEnumerator<T> : IReturnEnumerator<T>
    {
        [JsonIgnore]
        public T Current => this.List[Id];

        object IEnumerator.Current => this.Current!;

        public int Id { get; private set; } = -1;

        public List<T> List { get; private set; }

        public RewindEnumerator(IEnumerable<T> collection) => this.List = new List<T>(collection);

        [JsonConstructor]
        public RewindEnumerator(int id, List<T> list) => (this.Id, this.List) = (id, list);

        public void Dispose() {}

        public bool MoveNext()
        {
            if (this.Id < this.List.Count - 1)
            {
                this.Id++;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.Id > -1)
            {
                this.Id--;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this.Id = -1;
        }

        public void Return(T item)
        {
            if (this.Id > -1)
            {
                this.Id--;
            }
        }
    }
}