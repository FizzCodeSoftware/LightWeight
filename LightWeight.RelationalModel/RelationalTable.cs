namespace FizzCode.LightWeight.RelationalModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using FizzCode.LightWeight.Collections;

    [DebuggerDisplay("{SchemaAndName}")]
    public class RelationalTable
    {
        public RelationalSchema Schema { get; private set; }
        public string Name { get; private set; }
        public string SchemaAndName { get; private set; }

        public RelationalColumn this[string columnName] => _columns[columnName];

        public IEnumerable<string> FlagList => _flags?.Values ?? Enumerable.Empty<string>();
        public IEnumerable<AdditionalData> AdditionalDataList => _additionalData?.Values ?? Enumerable.Empty<AdditionalData>();
        public IReadOnlyList<RelationalColumn> Columns => _columns.GetItemsAsReadonly();
        public IReadOnlyList<RelationalColumn> PrimaryKeyColumns => _primaryKeyColumns.GetItemsAsReadonly();
        public IReadOnlyList<RelationalForeignKey> ForeignKeys => _foreignKeys.AsReadOnly();

        public bool AnyPrimaryKeyColumnIsIdentity => _primaryKeyColumns.Any(x => x.IsIdentity);

        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn> _columns = new();
        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn> _primaryKeyColumns = new();
        private readonly CaseInsensitiveStringKeyDictionary<List<RelationalColumn>> _columnsByFlags = new();

        private CaseInsensitiveStringKeyDictionary<string> _flags;
        private CaseInsensitiveStringKeyDictionary<AdditionalData> _additionalData;
        private readonly List<RelationalForeignKey> _foreignKeys = new();

        internal void Initialize(RelationalSchema schema, string name)
        {
            Schema = schema;
            Name = name;

            SchemaAndName = Schema.Name != null
                ? Schema.Name + "." + name
                : name;
        }

        public RelationalColumn AddColumn(string name, bool partOfPrimaryKey, int? index = null)
        {
            var column = new RelationalColumn(this, name, partOfPrimaryKey);

            if (index == null)
            {
                _columns.Add(name, column);
            }
            else if (!_columns.Insert(name, column, index.Value))
            {
                return null;
            }

            if (partOfPrimaryKey)
            {
                if (index == null)
                {
                    _primaryKeyColumns.Add(name, column);
                }
                else
                {
                    _primaryKeyColumns.Clear();
                    foreach (var col in _columns)
                    {
                        if (col.IsPrimaryKey)
                            _primaryKeyColumns.Add(col.Name, col);
                    }
                }
            }

            return column;
        }

        public RelationalForeignKey AddForeignKeyTo(RelationalTable targetTable)
        {
            var fk = new RelationalForeignKey(this, targetTable);
            _foreignKeys.Add(fk);

            return fk;
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
                    Schema.Model.HandleTableFlagged(this, flag, true);
                }
            }
            else if (_flags != null)
            {
                if (_flags.ContainsKey(flag))
                {
                    _flags.Remove(flag);
                    Schema.Model.HandleTableFlagged(this, flag, false);
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

        internal void HandleColumnFlagged(RelationalColumn column, string flag, bool value)
        {
            var flaggedColumns = _columnsByFlags[flag];
            if (flaggedColumns == null)
            {
                if (value)
                {
                    _columnsByFlags[flag] = flaggedColumns = new List<RelationalColumn>();
                    flaggedColumns.Add(column);
                }
            }
            else
            {
                flaggedColumns.Remove(column);
            }
        }

        public List<RelationalColumn> GetColumnsWithFlag(string flag)
        {
            var result = new List<RelationalColumn>();
            var columns = _columnsByFlags[flag];
            if (columns != null)
                result.AddRange(columns);

            return result;
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
}