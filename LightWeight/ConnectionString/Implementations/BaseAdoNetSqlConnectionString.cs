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
        return Fields?.Server != null
            ? Fields?.Port != null
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1} ({2}:{3}), {4}", Name, Fields.Database ?? "???", Fields.Server, Fields.Port, SqlEngine.ToString())
                : string.Format(CultureInfo.InvariantCulture, "{0}:{1} ({2}), {3}", Name, Fields.Database ?? "???", Fields.Server, SqlEngine.ToString())
            : string.Format(CultureInfo.InvariantCulture, "{0}:{1}, {2}", Name, Fields.Database ?? "???", SqlEngine.ToString());
    }

    protected abstract AdoNetConnectionStringFields GetFields();
    public abstract bool IsEscaped(string identifier);
    public abstract string Escape(string dbObject, string schema = null);
    public abstract string EscapeIdentifier(string identifier);
    public abstract string Unescape(string identifier);
    public abstract string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier);
    public abstract string GetObjectIdentifier(string fullIdentifier);
}