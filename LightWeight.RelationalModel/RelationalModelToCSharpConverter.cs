namespace FizzCode.LightWeight.RelationalModel
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class RelationalModelToCSharpConverter
    {
        public static void GenerateCSharpFile(string fileName, RelationalModel model, string @namespace, string modelClassName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("#pragma warning disable CA1034 // Nested types should not be visible");
            builder.AppendLine("#pragma warning disable CA1720 // Identifiers should not contain type names");
            builder.Append("namespace ").AppendLine(@namespace);
            builder.AppendLine("{");
            builder.AppendLine("\tusing FizzCode.LightWeight.RelationalModel;");
            builder.AppendLine();
            builder.Append("\tpublic class ").Append(modelClassName).AppendLine(" : RelationalModel");
            builder.AppendLine("\t{");

            foreach (var schema in model.Schemas)
            {
                builder.Append("\t\tpublic ").Append(schema.Name).Append("Schema ").Append(schema.Name).AppendLine(" { get; set; }");
            }

            builder.AppendLine();

            builder.Append("\t\tpublic ").Append(modelClassName).AppendLine("()");
            //builder.AppendLine("\t\t\t: base()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tBuildFromProperties();");
            builder.AppendLine("\t\t}");

            builder.AppendLine();

            foreach (var schema in model.Schemas)
            {
                var schemaClassName = schema.Name + "Schema";

                builder.Append("\t\tpublic class ").Append(schemaClassName).AppendLine(" : RelationalSchema");
                builder.AppendLine("\t\t{");
                foreach (var table in schema.Tables)
                {
                    builder.Append("\t\t\tpublic ").Append(table.Name).Append("Table ").Append(table.Name).AppendLine(" { get; set; }");
                }

                builder.AppendLine();

                var first = true;
                foreach (var table in schema.Tables)
                {
                    if (!first)
                        builder.AppendLine();

                    first = false;

                    foreach (var flag in table.FlagList)
                    {
                        builder.Append("\t\t\t[Flag(\"").Append(flag).AppendLine("\", true)]");
                    }

                    foreach (var additionalData in table.AdditionalDataList)
                    {
                        if (additionalData.Value is int iv)
                        {
                            builder.Append("\t\t\t[AdditionalData(\"").Append(additionalData.Key).Append("\", ").Append(iv.ToString("D", CultureInfo.InvariantCulture)).AppendLine(")]");
                        }
                        else if (additionalData.Value is string sv)
                        {
                            builder.Append("\t\t\t[AdditionalData(\"").Append(additionalData.Key).Append("\", \"").Append(sv).AppendLine("\")]");
                        }
                    }

                    var tableClassName = table.Name + "Table";

                    builder.Append("\t\t\tpublic class ").Append(tableClassName).AppendLine(" : RelationalTable");
                    builder.AppendLine("\t\t\t{");

                    foreach (var column in table.Columns)
                    {
                        if (column.IsPrimaryKey)
                            builder.AppendLine("\t\t\t\t[PrimaryKey]");

                        foreach (var fk in table.ForeignKeys.Where(fk => fk.ColumnPairs.Count == 1 && fk.ColumnPairs[0].SourceColumn == column))
                        {
                            builder
                                .Append("\t\t\t\t[SingleColumnForeignKey(typeof(")
                                .Append(fk.ColumnPairs[0].TargetColumn.Table.Schema.Name)
                                .Append("Schema.")
                                .Append(fk.ColumnPairs[0].TargetColumn.Table.Name)
                                .Append("Table), nameof(")
                                .Append(fk.ColumnPairs[0].TargetColumn.Table.Schema.Name)
                                .Append("Schema.")
                                .Append(fk.ColumnPairs[0].TargetColumn.Table.Name)
                                .Append("Table.")
                                .Append(fk.ColumnPairs[0].TargetColumn.Name)
                                .AppendLine("))]");
                        }

                        foreach (var flag in column.FlagList)
                        {
                            builder.Append("\t\t\t\t[Flag(\"").Append(flag).AppendLine("\", true)]");
                        }

                        var newPrefix = "";
                        if (column.Name == "Name")
                            newPrefix = "new ";

                        builder.Append("\t\t\t\tpublic ").Append(newPrefix).Append("RelationalColumn ").Append(column.Name).AppendLine(" { get; set; }");
                    }

                    builder.AppendLine("\t\t\t}");
                }

                builder.AppendLine("\t\t}");
            }

            builder.AppendLine("\t}");
            builder.AppendLine("}");
            builder.AppendLine("#pragma warning restore CA1034 // Nested types should not be visible");
            builder.Append("#pragma warning restore CA1720 // Identifiers should not contain type names");
            var content = builder.ToString().Replace("\t", "    ", StringComparison.Ordinal);
            File.WriteAllText(fileName, content);
        }
    }
}
