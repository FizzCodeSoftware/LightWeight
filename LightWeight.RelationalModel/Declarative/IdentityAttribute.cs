namespace FizzCode.LightWeight.RelationalModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IdentityAttribute : Attribute
    {
    }
}
