namespace FizzCode.LightWeight.RelationalModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using FizzCode.LightWeight.Collections;

    public class RelationalModel
    {
        public string DefaultSchema { get; }

        public IReadOnlyList<RelationalTable> Tables => _tables.GetItemsAsReadonly();
        public RelationalTable this[string name] => _tables[name];

        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalTable> _tables = new OrderedCaseInsensitiveStringKeyDictionary<RelationalTable>();
        private readonly CaseInsensitiveStringKeyDictionary<List<RelationalTable>> _tablesByFlags = new CaseInsensitiveStringKeyDictionary<List<RelationalTable>>();

        public RelationalModel(string defaultSchemaName = null)
        {
            DefaultSchema = defaultSchemaName;
        }

        protected void BuildFromProperties()
        {
            var allSingleColumnFkAttributes = new List<Tuple<RelationalColumn, List<SingleColumnForeignKeyAttribute>>>();

            foreach (var tableProperty in GetType().GetProperties())
            {
                if (typeof(RelationalTable).IsAssignableFrom(tableProperty.PropertyType) && tableProperty.DeclaringType == GetType())
                {
                    var table = (RelationalTable)Activator.CreateInstance(tableProperty.PropertyType);
                    var schemaAttribute = tableProperty.GetCustomAttribute<SchemaAttribute>();

                    table.Initialize(this, tableProperty.Name, schemaAttribute?.Schema);
                    _tables.Add(table.SchemaAndName, table);
                    tableProperty.SetValue(this, table);

                    foreach (var columnProperty in tableProperty.PropertyType.GetProperties())
                    {
                        if (typeof(RelationalColumn).IsAssignableFrom(columnProperty.PropertyType) && columnProperty.DeclaringType == tableProperty.PropertyType)
                        {
                            var primaryKeyAttribute = columnProperty.GetCustomAttribute<PrimaryKeyAttribute>();
                            var column = table.AddColumn(columnProperty.Name, primaryKeyAttribute != null);
                            columnProperty.SetValue(table, column);

                            var identityAttribute = columnProperty.GetCustomAttribute<IdentityAttribute>();
                            column.SetIdentity(identityAttribute != null);

                            var fkAttributes = columnProperty.GetCustomAttributes<SingleColumnForeignKeyAttribute>().ToList();
                            if (fkAttributes.Count > 0)
                            {
                                allSingleColumnFkAttributes.Add(new Tuple<RelationalColumn, List<SingleColumnForeignKeyAttribute>>(column, fkAttributes));
                            }

                            var flagAttributes = columnProperty.GetCustomAttributes<FlagAttribute>();
                            foreach (var flagAttribute in flagAttributes)
                            {
                                if (flagAttribute.Exclusive)
                                {
                                    if (column.Table.GetColumnsWithFlag(flagAttribute.Name).Count > 0)
                                    {
                                        throw new Exception(string.Format(CultureInfo.InvariantCulture, "more than 1 column uses the same exclusive flag: {0}", flagAttribute.Name));
                                    }
                                }

                                column.SetFlag(flagAttribute.Name, flagAttribute.Value, flagAttribute.Exclusive);
                            }
                        }
                    }
                }
            }

            foreach (var t in allSingleColumnFkAttributes)
            {
                var sourceColumn = t.Item1;
                var fkAttributes = t.Item2;
                foreach (var fkAttribute in fkAttributes)
                {
                    var tableProperty = GetType().GetProperties().FirstOrDefault(x => x.PropertyType == fkAttribute.TargetTableType);
                    var schemaAttribute = tableProperty.GetCustomAttribute<SchemaAttribute>();

                    var schema = schemaAttribute?.Schema ?? DefaultSchema;

                    var targetSchemaAndName = (schema != null ? schema + "." : "") + tableProperty.Name;
                    var targetTable = _tables[targetSchemaAndName];
                    var targetColumn = targetTable[fkAttribute.TargetColumnName];

                    sourceColumn.Table.AddForeignKeyTo(targetTable)
                        .AddColumnPair(sourceColumn, targetColumn);
                }
            }
        }

        public RelationalTable AddTable(string name, string customSchema = null)
        {
            var table = new RelationalTable();
            table.Initialize(this, name, customSchema);
            _tables.Add(table.SchemaAndName, table);
            return table;
        }

        internal void HandleTableFlagged(RelationalTable table, string flag, bool value)
        {
            var flaggedTables = _tablesByFlags[flag];
            if (flaggedTables == null)
            {
                if (value)
                {
                    _tablesByFlags[flag] = flaggedTables = new List<RelationalTable>();
                    flaggedTables.Add(table);
                }
            }
            else
            {
                flaggedTables.Remove(table);
            }
        }

        public List<RelationalTable> GetTablesWithFlag(string flag)
        {
            var result = new List<RelationalTable>();
            var tables = _tablesByFlags[flag];
            if (tables != null)
                result.AddRange(tables);

            return result;
        }
    }
}