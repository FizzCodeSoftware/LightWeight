namespace FizzCode.LightWeight.Collections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class OrderedCaseInsensitiveStringKeyDictionary<TItem> : IEnumerable<TItem>
    {
        private readonly List<Entry> _entriesOrdered = new List<Entry>();
        private readonly Dictionary<string, Entry> _entriesByUpperKey = new Dictionary<string, Entry>();

        public IReadOnlyList<TItem> GetItemsAsReadonly()
        {
            return _entriesOrdered.Select(x => x.Item).ToList().AsReadOnly();
        }

        public int Count => _entriesByUpperKey.Count;

        public TItem this[string key]
        {
            get
            {
                if (_entriesByUpperKey.TryGetValue(key.ToUpperInvariant(), out var entry))
                    return entry.Item;

                return default;
            }
            set => Add(key, value);
        }

        public void Clear()
        {
            _entriesOrdered.Clear();
            _entriesByUpperKey.Clear();
        }

        public void Add(string key, TItem item)
        {
            var upperKey = key.ToUpperInvariant();
            if (_entriesByUpperKey.TryGetValue(upperKey, out var entry))
            {
                entry.Item = item;
            }
            else
            {
                entry = new Entry()
                {
                    Key = key,
                    UpperKey = upperKey,
                    Item = item,
                };

                _entriesOrdered.Add(entry);
                _entriesByUpperKey[upperKey] = entry;
            }
        }

        public bool Insert(string key, TItem item, int index)
        {
            var upperKey = key.ToUpperInvariant();
            if (_entriesByUpperKey.ContainsKey(upperKey))
                return false;

            if (index < 0 || index > _entriesOrdered.Count)
                return false;

            var entry = new Entry()
            {
                Key = key,
                UpperKey = upperKey,
                Item = item,
            };

            _entriesOrdered.Insert(index, entry);
            _entriesByUpperKey[upperKey] = entry;
            return true;
        }

        public void Remove(string key)
        {
            var upperKey = key.ToUpperInvariant();
            if (_entriesByUpperKey.TryGetValue(upperKey, out var item))
            {
                _entriesByUpperKey.Remove(upperKey);

                var index = _entriesOrdered.FindIndex(x => x.UpperKey == upperKey);
                _entriesOrdered.RemoveAt(index);
            }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _entriesOrdered.Select(x => x.Item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entriesOrdered.Select(x => x.Item).GetEnumerator();
        }

        private class Entry
        {
            public string Key { get; set; }
            public string UpperKey { get; set; }
            public TItem Item { get; set; }
        }
    }
}