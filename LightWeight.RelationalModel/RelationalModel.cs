using System.Reflection;

namespace FizzCode.LightWeight.RelationalModel;

public class RelationalModel
{
    public RelationalSchema DefaultSchema { get; private set; }

    public IReadOnlyList<RelationalSchema> Schemas => _schemas.GetItemsAsReadonly();
    public RelationalSchema this[string schemaName] => _schemas[schemaName];

    private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalSchema> _schemas = new();
    private readonly CaseInsensitiveStringKeyDictionary<List<RelationalTable>> _tablesByFlags = new();

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

            if (DefaultSchema == null)
                DefaultSchema = schema;

            foreach (var tableProperty in schemaProperty.PropertyType.GetProperties())
            {
                if (!typeof(RelationalTable).IsAssignableFrom(tableProperty.PropertyType) || tableProperty.DeclaringType != schemaProperty.PropertyType)
                    continue;

                var table = (RelationalTable)Activator.CreateInstance(tableProperty.PropertyType);
                schema.AddTable(table, tableProperty.Name);
                tableProperty.SetValue(schema, table);

                var flagAttributes = tableProperty.PropertyType.GetCustomAttributes<FlagAttribute>();
                foreach (var flagAttribute in flagAttributes)
                {
                    table.SetFlag(flagAttribute.Name, flagAttribute.Value);
                }

                var additionalDataAttributes = tableProperty.PropertyType.GetCustomAttributes<AdditionalDataAttribute>();
                foreach (var additionalDataAttribute in additionalDataAttributes)
                {
                    table.SetAdditionalData(additionalDataAttribute.Name, additionalDataAttribute.Value);
                }

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

                    flagAttributes = columnProperty.GetCustomAttributes<FlagAttribute>();
                    foreach (var flagAttribute in flagAttributes)
                    {
                        column.SetFlag(flagAttribute.Name, flagAttribute.Value);
                    }

                    additionalDataAttributes = columnProperty.GetCustomAttributes<AdditionalDataAttribute>();
                    foreach (var additionalDataAttribute in additionalDataAttributes)
                    {
                        column.SetAdditionalData(additionalDataAttribute.Name, additionalDataAttribute.Value);
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
