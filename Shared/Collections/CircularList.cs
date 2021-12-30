using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Collections
{
    public class CircularList<T> : SimpleList<T>
    {
        public CircularList() : base() {}
        
        public CircularList(IList<T> list) : base(list) {}

        public override bool MoveNext()
        {
            this.CurrentId = (this.CurrentId + 1) % this.List.Count;
            return true;
        }
    }
}