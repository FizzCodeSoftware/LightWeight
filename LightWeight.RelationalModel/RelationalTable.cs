namespace FizzCode.LightWeight.RelationalModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using FizzCode.LightWeight.Collections;

    [DebuggerDisplay("{SchemaAndName}")]
    public class RelationalTable
    {
        public RelationalModel Model { get; private set; }
        public string Name { get; private set; }
        public string Schema { get; private set; }
        public string SchemaAndName { get; private set; }

        public RelationalColumn this[string columnName] => _columns[columnName];

        public IReadOnlyList<RelationalColumn> Columns => _columns.GetItemsAsReadonly();
        public IReadOnlyList<RelationalColumn> PrimaryKeyColumns => _primaryKeyColumns.GetItemsAsReadonly();
        public IReadOnlyList<RelationalForeignKey> ForeignKeys => _foreignKeys.AsReadOnly();

        public bool AnyPrimaryKeyColumnIsIdentity => _primaryKeyColumns.Any(x => x.IsIdentity);

        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn> _columns = new OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn>();
        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn> _primaryKeyColumns = new OrderedCaseInsensitiveStringKeyDictionary<RelationalColumn>();
        private readonly CaseInsensitiveStringKeyDictionary<List<RelationalColumn>> _columnsByFlags = new CaseInsensitiveStringKeyDictionary<List<RelationalColumn>>();

        private CaseInsensitiveStringKeyDictionary<bool> _flags;
        private CaseInsensitiveStringKeyDictionary<object> _additionalData;
        private readonly List<RelationalForeignKey> _foreignKeys = new List<RelationalForeignKey>();

        internal void Initialize(RelationalModel model, string name, string customSchema)
        {
            Model = model;
            Name = name;
            Schema = customSchema ?? model.DefaultSchema;

            SchemaAndName = Schema != null
                ? Schema + "." + name
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

        public void SetFlag(string flag, bool value, bool exclusive)
        {
            if (value)
            {
                if (_flags == null)
                    _flags = new CaseInsensitiveStringKeyDictionary<bool>();

                if (!_flags[flag])
                {
                    _flags[flag] = true;
                    Model.HandleTableFlagged(this, flag, true);
                }

                if (exclusive)
                {
                    foreach (var t in Model.GetTablesWithFlag(flag))
                    {
                        if (t != this)
                            t.SetFlag(flag, false, false);
                    }
                }
            }
            else if (_flags != null)
            {
                if (_flags[flag])
                {
                    _flags.Remove(flag);
                    Model.HandleTableFlagged(this, flag, false);
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