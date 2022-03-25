namespace FizzCode.LightWeight.RelationalModel;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public class AdditionalDataAttribute : Attribute
{
    public string Name { get; }
    public object Value { get; }

    public AdditionalDataAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public AdditionalDataAttribute(string name, int value)
    {
        Name = name;
        Value = value;
    }
}
