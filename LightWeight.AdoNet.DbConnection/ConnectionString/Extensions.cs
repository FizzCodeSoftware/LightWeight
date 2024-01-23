// these extension methods are global, namespace shouldn't be used
#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
using FizzCode.LightWeight;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class Extensions
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static bool IsEscaped(this NamedConnectionString connectionString, string identifier)
    {
        return connectionString.GetAdoNetHelper()?.IsEscaped(identifier) == true;
    }

    public static string Escape(this NamedConnectionString connectionString, string dbObject, string schema = null)
    {
        return connectionString.GetAdoNetHelper()?.Escape(dbObject, schema);
    }

    public static string Unescape(this NamedConnectionString connectionString, string identifier)
    {
        return connectionString.GetAdoNetHelper()?.Unescape(identifier) ?? identifier;
    }

    public static AdoNetEngine? GetAdoNetEngine(this NamedConnectionString connectionString)
    {
        return connectionString.GetAdoNetHelper()?.Engine;
    }

    public static string GetFriendlyProviderName(this NamedConnectionString connectionString)
    {
        return connectionString.GetAdoNetHelper()?.Engine.ToString() ?? connectionString.ProviderName;
    }
}