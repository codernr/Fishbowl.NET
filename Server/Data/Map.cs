using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Fishbowl.Net.Server.Data
{
    public interface IMap<T1, T2> : ICollection<(T1, T2)>, IEnumerable<(T1, T2)>, IEnumerable
        where T1 : notnull where T2 : notnull
    {
        T2 this[T1 key] { get; set; }

        T1 this[T2 key] { get; set; }

        ICollection<T1> Items1 { get; }

        ICollection<T2> Items2 { get; }

        void Add(T1 item1, T2 item2);

        bool ContainsKey(T1 key);

        bool ContainsKey(T2 key);

        bool Remove(T1 key);

        bool Remove(T2 key);

        bool TryGetValue(T1 key, [MaybeNullWhen(false)] out T2 value);

        bool TryGetValue(T2 key, [MaybeNullWhen(false)] out T1 value);
    }

    public class Map<T1, T2> : IMap<T1, T2> where T1 : notnull where T2 : notnull
    {
        private readonly Dictionary<T1, T2> forward = new();

        private readonly Dictionary<T2, T1> reverse = new();

        public T2 this[T1 key]
        {
            get => this.forward[key];
            set => this.forward[key] = value;
        }
        
        public T1 this[T2 key]
        {
            get => this.reverse[key];
            set => this.reverse[key] = value;
        }

        public ICollection<T1> Items1 => this.forward.Keys;

        public ICollection<T2> Items2 => this.reverse.Keys;

        public int Count => this.forward.Count;

        public bool IsReadOnly => false;

        public void Add(T1 item1, T2 item2)
        {
            this.forward.Add(item1, item2);
            this.reverse.Add(item2, item1);
        }

        public void Add((T1, T2) item)
        {
            this.forward.Add(item.Item1, item.Item2);
            this.reverse.Add(item.Item2, item.Item1);
        }

        public void Clear()
        {
            this.forward.Clear();
            this.reverse.Clear();
        }

        public bool Contains((T1, T2) item) => this.forward.ContainsKey(item.Item1) && this.reverse.ContainsKey(item.Item2);

        public bool ContainsKey(T1 key) => this.forward.ContainsKey(key);

        public bool ContainsKey(T2 key) => this.reverse.ContainsKey(key);

        public void CopyTo((T1, T2)[] array, int arrayIndex) => this.forward
            .Select(c => (c.Key, c.Value))
            .ToList()
            .CopyTo(array, arrayIndex);

        public IEnumerator<(T1, T2)> GetEnumerator() => this.forward
            .Select(c => (c.Key, c.Value))
            .GetEnumerator();

        public bool Remove(T1 key)
        {
            var key2 = this.forward[key];
            return this.forward.Remove(key) && this.reverse.Remove(key2);
        }

        public bool Remove(T2 key)
        {
            var key2 = this.reverse[key];
            return this.reverse.Remove(key) && this.forward.Remove(key2);
        }

        public bool Remove((T1, T2) item) =>
            this.forward.Remove(item.Item1) && this.reverse.Remove(item.Item2);

        public bool TryGetValue(T1 key, [MaybeNullWhen(false)] out T2 value) => this.forward.TryGetValue(key, out value);

        public bool TryGetValue(T2 key, [MaybeNullWhen(false)] out T1 value) => this.reverse.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}