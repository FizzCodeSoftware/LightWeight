namespace FizzCode.LightWeight.RelationalModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class FlagAttribute : Attribute
    {
        public string Name { get; }
        public bool Value { get; }

        public FlagAttribute(string name, bool value)
        {
            Name = name;
            Value = value;
        }
    }
}