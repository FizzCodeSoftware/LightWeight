namespace FizzCode;

public abstract class BaseAdoNetSqlConnectionString : IAdoNetSqlConnectionString
{
    public string Name { get; }
    public string ConnectionString { get; }
    public string Version { get; }
    public AdoNetEngine SqlEngine { get; }
    public string ProviderName { get; }
    public AdoNetConnectionStringFields Fields { get; }

    protected BaseAdoNetSqlConnectionString(AdoNetEngine sqlEngine, string providerName, string name, string connectionString, string version = null)
    {
        Name = name;
        ConnectionString = connectionString;
        Version = version;
        SqlEngine = sqlEngine;
        ProviderName = providerName;
        Fields = GetFields();
    }

    public override string ToString()
    {
        return Fields?.Database != null
            ? Fields?.Server != null
                ? Fields?.Port != null
                    ? string.Format(CultureInfo.InvariantCulture, "{0} ({1}:{2}, {3}), {4}", Name, Fields.Server, Fields.Port, Fields.Database, ProviderName)
                    : string.Format(CultureInfo.InvariantCulture, "{0} ({1}, {2}), {3}", Name, Fields.Server, Fields.Database, ProviderName)
                : string.Format(CultureInfo.InvariantCulture, "{0} ({1}), {2}", Name, Fields.Database, ProviderName)
            : string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }

    protected abstract AdoNetConnectionStringFields GetFields();
    public abstract bool IsEscaped(string identifier);
    public abstract string Escape(string dbObject, string schema = null);
    public abstract string EscapeIdentifier(string identifier);
    public abstract string Unescape(string identifier);
    public abstract string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier);
    public abstract string GetObjectIdentifier(string fullIdentifier);
}