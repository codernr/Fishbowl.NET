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
            this.CurrentId = (this.CurrentId + 1) % this.List.Count;
            return true;
        }
    }
}