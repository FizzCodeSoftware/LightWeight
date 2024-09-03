namespace FizzCode.LightWeight.AdoNet;

public interface IAdoNetConnectionStringHelper
{
    string ProviderName { get; }
    AdoNetEngine Engine { get; }
    AdoNetConnectionStringFields GetKnownConnectionStringFields(GenericNamedConnectionString connectionString);
    bool IsEscaped(string identifier);
    string Escape(string dbObject, string schema = null);
    string EscapeIdentifier(string identifier);
    string Unescape(string identifier);

    string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier);
    string GetObjectIdentifier(string fullIdentifier);
}