namespace FizzCode.LightWeight.RelationalModel;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FizzCode.LightWeight.Collections;

[DebuggerDisplay("{TableQualifiedName}")]
public class RelationalColumn
{
    public RelationalTable Table { get; }
    public string Name { get; }
    public bool IsPrimaryKey { get; }

    public bool IsIdentity { get; private set; }

    public IEnumerable<string> FlagList => _flags?.Values ?? Enumerable.Empty<string>();
    public IEnumerable<AdditionalData> AdditionalDataList => _additionalData?.Values ?? Enumerable.Empty<AdditionalData>();

    private CaseInsensitiveStringKeyDictionary<string> _flags;
    private CaseInsensitiveStringKeyDictionary<AdditionalData> _additionalData;

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

    public void SetFlag(string flag, bool value)
    {
        if (value)
        {
            if (_flags == null)
                _flags = new CaseInsensitiveStringKeyDictionary<string>();

            if (!_flags.ContainsKey(flag))
            {
                _flags[flag] = flag;
                Table.HandleColumnFlagged(this, flag, true);
            }
        }
        else if (_flags != null)
        {
            if (_flags.ContainsKey(flag))
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

        return _flags.ContainsKey(flag);
    }

    public void SetAdditionalData(string key, object value)
    {
        if (_additionalData == null)
            _additionalData = new CaseInsensitiveStringKeyDictionary<AdditionalData>();

        _additionalData[key] = new AdditionalData(key, value);
    }

    public object GetAdditionalData(string key)
    {
        if (_additionalData == null)
            return null;

        return _additionalData[key]?.Value;
    }
}
