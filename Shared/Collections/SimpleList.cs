using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Collections
{
    public class SimpleList<T> : IGameList<T>
    {
        [JsonInclude]
        public int CurrentId { get; protected set; } = -1;

        [JsonInclude]
        public IList<T> List { get; private set; } = default!;

        [JsonIgnore]
        public T Current => this.List[this.CurrentId];

        public SimpleList() {}

        public SimpleList(IList<T> list) => this.List = list;

        public virtual bool MoveNext()
        {
            if (this.CurrentId < this.List.Count - 1)
            {
                this.CurrentId++;
                return true;
            }

            return false;
        }
    }
}