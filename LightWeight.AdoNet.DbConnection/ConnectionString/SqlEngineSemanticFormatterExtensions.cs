// these extension methods are global, namespace shouldn't be used
#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
public static class SqlEngineSemanticFormatterExtensions
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static ConnectionStringFields GetKnownConnectionStringFields(this NamedConnectionString connectionString)
    {
        return SqlEngineSemanticFormatter.GetFormatter(connectionString).GetKnownConnectionStringFields(connectionString);
    }

    public static bool IsEscaped(this NamedConnectionString connectionString, string identifier)
    {
        return SqlEngineSemanticFormatter.GetFormatter(connectionString).IsEscaped(identifier);
    }

    public static string Escape(this NamedConnectionString connectionString, string dbObject, string schema = null)
    {
        return SqlEngineSemanticFormatter.GetFormatter(connectionString).Escape(dbObject, schema);
    }

    public static string Unescape(this NamedConnectionString connectionString, string identifier)
    {
        return SqlEngineSemanticFormatter.GetFormatter(connectionString).Unescape(identifier);
    }
}