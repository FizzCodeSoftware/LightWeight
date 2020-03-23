namespace FizzCode.LightWeight.RelationalModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using FizzCode.LightWeight.Collections;

    [DebuggerDisplay("{Name}")]
    public class RelationalSchema
    {
        public RelationalModel Model { get; private set; }
        public string Name { get; private set; }

        public IReadOnlyList<RelationalTable> Tables => _tables.GetItemsAsReadonly();
        public RelationalTable this[string tableName] => _tables[tableName];

        private readonly OrderedCaseInsensitiveStringKeyDictionary<RelationalTable> _tables = new OrderedCaseInsensitiveStringKeyDictionary<RelationalTable>();

        public void Initialize(RelationalModel model, string name = null)
        {
            Model = model;
            Name = name;
        }

        public RelationalTable AddTable(string name)
        {
            var table = new RelationalTable();
            table.Initialize(this, name);
            _tables.Add(table.Name, table);
            return table;
        }

        internal void AddTable(RelationalTable table, string name)
        {
            table.Initialize(this, name);
            _tables.Add(table.Name, table);
        }
    }
}