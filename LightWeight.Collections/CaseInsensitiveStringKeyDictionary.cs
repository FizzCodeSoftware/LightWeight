namespace FizzCode.LightWeight.Collections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class CaseInsensitiveStringKeyDictionary<T> : IEnumerable<T>
    {
        private readonly Dictionary<string, T> _itemsByUpperKey = new Dictionary<string, T>();

        public IReadOnlyList<T> GetItemsAsReadonly()
        {
            return _itemsByUpperKey.Values.ToList().AsReadOnly();
        }

        public int Count => _itemsByUpperKey.Count;

        public T this[string key]
        {
            get
            {
                _itemsByUpperKey.TryGetValue(key.ToUpperInvariant(), out var item);
                return item;
            }
            set => Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _itemsByUpperKey.ContainsKey(key.ToUpperInvariant());
        }

        public void Add(string key, T value)
        {
            _itemsByUpperKey[key.ToUpperInvariant()] = value;
        }

        public void Remove(string key)
        {
            _itemsByUpperKey.Remove(key.ToUpperInvariant());
        }

        public void Clear()
        {
            _itemsByUpperKey.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _itemsByUpperKey.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _itemsByUpperKey.Values.GetEnumerator();
        }
    }
}