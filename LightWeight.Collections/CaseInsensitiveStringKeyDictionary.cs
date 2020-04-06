namespace FizzCode.LightWeight.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class CaseInsensitiveStringKeyDictionary<T> : IEnumerable<T>
    {
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        public IReadOnlyList<T> GetItemsAsReadonly()
        {
            return _items.Values.ToList().AsReadOnly();
        }

        public int Count => _items.Count;

        public T this[string key]
        {
            get
            {
                _items.TryGetValue(key, out var item);
                return item;
            }
            set => Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public void Add(string key, T value)
        {
            _items[key] = value;
        }

        public void Remove(string key)
        {
            _items.Remove(key);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }
    }
}