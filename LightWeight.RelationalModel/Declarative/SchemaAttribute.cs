namespace FizzCode.LightWeight.RelationalModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SchemaAttribute : Attribute
    {
        public string Schema { get; }

        public SchemaAttribute(string schema)
        {
            Schema = schema;
        }
    }
}