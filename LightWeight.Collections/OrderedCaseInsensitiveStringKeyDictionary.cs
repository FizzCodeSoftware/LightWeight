namespace FizzCode.LightWeight.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class OrderedCaseInsensitiveStringKeyDictionary<TItem> : IEnumerable<TItem>
    {
        private readonly List<Entry> _entriesOrdered = new List<Entry>();
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<TItem> GetItemsAsReadonly()
        {
            return _entriesOrdered.Select(x => x.Item).ToList().AsReadOnly();
        }

        public int Count => _entries.Count;

        public TItem this[string key]
        {
            get
            {
                if (_entries.TryGetValue(key, out var entry))
                    return entry.Item;

                return default;
            }
            set => Add(key, value);
        }

        public void Clear()
        {
            _entriesOrdered.Clear();
            _entries.Clear();
        }

        public void Add(string key, TItem item)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                entry.Item = item;
            }
            else
            {
                entry = new Entry()
                {
                    Key = key,
                    Item = item,
                };

                _entriesOrdered.Add(entry);
                _entries[key] = entry;
            }
        }

        public bool Insert(string key, TItem item, int index)
        {
            if (_entries.ContainsKey(key))
                return false;

            if (index < 0 || index > _entriesOrdered.Count)
                return false;

            var entry = new Entry()
            {
                Key = key,
                Item = item,
            };

            _entriesOrdered.Insert(index, entry);
            _entries[key] = entry;
            return true;
        }

        public void Remove(string key)
        {
            if (_entries.Remove(key))
            {
                var index = _entriesOrdered.FindIndex(x => _entries.Comparer.Equals(x.Key, key));
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
            public TItem Item { get; set; }
        }
    }
}