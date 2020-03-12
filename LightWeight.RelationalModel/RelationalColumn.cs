namespace FizzCode.LightWeight.RelationalModel
{
    using System.Diagnostics;
    using FizzCode.LightWeight.Collections;

    [DebuggerDisplay("{TableQualifiedName}")]
    public class RelationalColumn
    {
        public RelationalTable Table { get; }
        public string Name { get; }
        public bool IsPrimaryKey { get; }

        public bool IsIdentity { get; private set; }

        private CaseInsensitiveStringKeyDictionary<bool> _flags;
        private CaseInsensitiveStringKeyDictionary<object> _additionalData;

        internal RelationalColumn(RelationalTable table, string name, bool isPrimaryKey)
        {
            Table = table;
            Name = name;
            IsPrimaryKey = isPrimaryKey;
        }

        public string TableQualifiedName => Table.Name + "." + Name;

        public RelationalColumn SetIdentity(bool value = true)
        {
            IsIdentity = value;
            return this;
        }

        public void SetFlag(string flag, bool value, bool exclusive)
        {
            if (value)
            {
                if (_flags == null)
                    _flags = new CaseInsensitiveStringKeyDictionary<bool>();

                if (!_flags[flag])
                {
                    _flags[flag] = true;
                    Table.HandleColumnFlagged(this, flag, true);
                }

                if (exclusive)
                {
                    foreach (var c in Table.GetColumnsWithFlag(flag))
                    {
                        if (c != this)
                            c.SetFlag(flag, false, false);
                    }
                }
            }
            else if (_flags != null)
            {
                if (_flags[flag])
                {
                    _flags.Remove(flag);
                    Table.HandleColumnFlagged(this, flag, false);
                }

                if (_flags.Count == 0)
                    _flags = null;
            }
        }

        public bool GetFlag(string flag)
        {
            if (_flags == null)
                return false;

            return _flags[flag];
        }

        public void SetAdditionalData(string key, object value)
        {
            if (_additionalData == null)
                _additionalData = new CaseInsensitiveStringKeyDictionary<object>();

            _additionalData[key] = value;
        }

        public object GetAdditionalData(string key)
        {
            if (_additionalData == null)
                return null;

            return _additionalData[key];
        }
    }
}