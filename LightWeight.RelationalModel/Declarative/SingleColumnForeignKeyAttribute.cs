namespace FizzCode.LightWeight.RelationalModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SingleColumnForeignKeyAttribute : Attribute
    {
        public string TargetTableName { get; }
        public string TargetColumnName { get; }

        public SingleColumnForeignKeyAttribute(string targetTableName, string targetColumn)
        {
            TargetTableName = targetTableName;
            TargetColumnName = targetColumn;
        }
    }
}