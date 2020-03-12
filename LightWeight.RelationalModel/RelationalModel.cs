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
        public RelationalSchema DefaultSchema { get; }

        public IReadOnlyList<RelationalSchema> Schemas => _schemas.GetItemsAsReadonly();
        public RelationalSchema this[string schemaName] => _schemas[schemaName];

        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalSchema> _schemas = new OrderedCaseInsensitiveStringKeyDictionary<RelationalSchema>();
        private readonly CaseInsensitiveStringKeyDictionary<List<RelationalTable>> _tablesByFlags = new CaseInsensitiveStringKeyDictionary<List<RelationalTable>>();

        public RelationalModel(string defaultSchemaName = null)
        {
            if (defaultSchemaName != null)
            {
                DefaultSchema = AddSchema(defaultSchemaName);
            }
        }

        protected void BuildFromProperties()
        {
            var allSingleColumnFkAttributes = new List<Tuple<RelationalColumn, List<SingleColumnForeignKeyAttribute>>>();

            foreach (var schemaProperty in GetType().GetProperties())
            {
                if (!typeof(RelationalSchema).IsAssignableFrom(schemaProperty.PropertyType) || schemaProperty.DeclaringType != GetType())
                    continue;

                var schema = (RelationalSchema)Activator.CreateInstance(schemaProperty.PropertyType);
                schema.Initialize(this, schemaProperty.Name);
                _schemas.Add(schema.Name, schema);

                schemaProperty.SetValue(this, schema);

                foreach (var tableProperty in schemaProperty.PropertyType.GetProperties())
                {
                    if (!typeof(RelationalTable).IsAssignableFrom(tableProperty.PropertyType) || tableProperty.DeclaringType != schemaProperty.PropertyType)
                        continue;

                    var table = (RelationalTable)Activator.CreateInstance(tableProperty.PropertyType);
                    schema.AddTable(table, tableProperty.Name);
                    tableProperty.SetValue(schema, table);

                    foreach (var columnProperty in tableProperty.PropertyType.GetProperties())
                    {
                        if (!typeof(RelationalColumn).IsAssignableFrom(columnProperty.PropertyType) || columnProperty.DeclaringType != tableProperty.PropertyType)
                            continue;

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

            foreach (var t in allSingleColumnFkAttributes)
            {
                var sourceColumn = t.Item1;
                var fkAttributes = t.Item2;
                foreach (var fkAttribute in fkAttributes)
                {
                    var schemaProperty = Array.Find(GetType().GetProperties(), x => x.PropertyType == fkAttribute.TargetTableType.DeclaringType);
                    var targetSchema = (RelationalSchema)schemaProperty.GetValue(this);

                    var tableProperty = Array.Find(schemaProperty.PropertyType.GetProperties(), x => x.PropertyType == fkAttribute.TargetTableType);
                    var targetTableName = tableProperty.Name;

                    var targetTable = targetSchema[targetTableName];
                    var targetColumn = targetTable[fkAttribute.TargetColumnName];

                    sourceColumn.Table.AddForeignKeyTo(targetTable)
                        .AddColumnPair(sourceColumn, targetColumn);
                }
            }
        }

        public RelationalSchema AddSchema(string name)
        {
            if (_schemas[name] != null)
                throw new ArgumentException("schema already exists in model", name);

            var schema = new RelationalSchema();
            schema.Initialize(this, name);
            _schemas.Add(schema.Name, schema);
            return schema;
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