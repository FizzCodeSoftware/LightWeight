namespace FizzCode.LightWeight.AdoNet;

public interface ISqlEngineSemanticFormatter
{
    string ProviderName { get; }
    SqlEngine SqlEngine { get; }
    ConnectionStringFields GetKnownConnectionStringFields(NamedConnectionString connectionString);
    bool IsEscaped(string identifier);
    string Escape(string dbObject, string schema = null);
    string EscapeIdentifier(string identifier);
    string Unescape(string identifier);

    string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier);
    string GetObjectIdentifier(string fullIdentifier);
}