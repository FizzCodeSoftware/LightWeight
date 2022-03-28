namespace FizzCode.LightWeight.RelationalModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class SingleColumnForeignKeyAttribute : Attribute
{
    public Type TargetTableType { get; }
    public string TargetColumnName { get; }

    public SingleColumnForeignKeyAttribute(Type targetTableType, string targetColumnName)
    {
        TargetTableType = targetTableType;
        TargetColumnName = targetColumnName;
    }
}
