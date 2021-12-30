using System.Collections.Generic;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.GameEntities
{
    public class ShuffleList<T> : IRewindList<T>
    {
        [JsonInclude]
        public Stack<T> PreviousItems { get; private set; } = new();

        [JsonInclude]
        public Stack<T> NextItems { get; private set; }

        [JsonIgnore]
        public T Current => this.PreviousItems.Peek();

        [JsonInclude]
        public bool Rewound { get; private set; } = false;

        public ShuffleList(IEnumerable<T> items) => this.NextItems = this.GetNextItems(items);

        public bool MoveNext()
        {
            if (this.NextItems.Count > 0)
            {
                this.PreviousItems.Push(this.NextItems.Pop());
                this.Rewound = false;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (this.PreviousItems.Count > 1 && !this.Rewound)
            {
                this.NextItems.Push(this.PreviousItems.Pop());
                this.NextItems = this.GetNextItems(this.NextItems);
                this.Rewound = true;
                return true;
            }

            return false;
        }

        protected virtual Stack<T> GetNextItems(IEnumerable<T> items) => new(items.Randomize());
    }
}