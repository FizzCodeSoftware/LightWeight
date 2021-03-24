namespace FizzCode.LightWeight.RelationalModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    [DebuggerDisplay("{DisplayNameWithSource}")]
    public class RelationalForeignKey
    {
        public RelationalTable SourceTable { get; }
        public RelationalTable TargetTable { get; }
        public IReadOnlyList<RelationalColumnPair> ColumnPairs => _columnPairs.AsReadOnly();

        private readonly List<RelationalColumnPair> _columnPairs = new();

        internal RelationalForeignKey(RelationalTable sourceTable, RelationalTable targetTable)
        {
            SourceTable = sourceTable;
            TargetTable = targetTable;
        }

        public void AddColumnPair(RelationalColumn sourceColumn, RelationalColumn targetColumn)
        {
            if (sourceColumn.Table != SourceTable)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "source column {0} is not part of the source table: {1}", sourceColumn.TableQualifiedName, SourceTable.Name), nameof(sourceColumn));

            if (targetColumn.Table != TargetTable)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "target column {0} is not part of the target table: {1}", targetColumn.TableQualifiedName, TargetTable.Name), nameof(targetColumn));

            _columnPairs.Add(new RelationalColumnPair(sourceColumn, targetColumn));
        }

        public string DisplayName => TargetTable.SchemaAndName + ": " + string.Join(", ", _columnPairs.Select(cp => cp.SourceColumn.Name + " -> " + cp.TargetColumn.Name));
        public string DisplayNameWithSource => SourceTable.SchemaAndName + " -> " + TargetTable.SchemaAndName + ": " + string.Join(", ", _columnPairs.Select(cp => cp.SourceColumn.Name + " -> " + cp.TargetColumn.Name));
    }
}