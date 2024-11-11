namespace FizzCode;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions IgnoreCSharpRequiredModifier(this JsonSerializerOptions options)
    {
        options.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();
        options.TypeInfoResolver = options.TypeInfoResolver.WithAddedModifier(info =>
        {
            foreach (var p in info.Properties)
            {
                var hasRequiredAttribute = p.AttributeProvider?.IsDefined(typeof(JsonRequiredAttribute), inherit: false) == true;
                p.IsRequired &= hasRequiredAttribute;
            }
        });

        return options;
    }
}