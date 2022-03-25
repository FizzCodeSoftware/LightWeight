namespace FizzCode.LightWeight.AdoNet;

using System.Diagnostics;
using System.Globalization;

[DebuggerDisplay("{Name}, {ProviderName}, {ConnectionString}")]
public class NamedConnectionString
{
    public string Name { get; }
    public string ProviderName { get; }
    public string ConnectionString { get; }
    public string Version { get; }
    public SqlEngine SqlEngine { get; }

    public NamedConnectionString(string name, string providerName, string connectionString, string version)
    {
        Name = name;
        ProviderName = providerName;
        ConnectionString = connectionString;
        Version = version;
        SqlEngine = SqlEngineSemanticFormatter.GetSqlEngineByProviderName(ProviderName);
    }

    public string GetFriendlyProviderName()
    {
        if (SqlEngine != SqlEngine.Generic)
            return SqlEngine.ToString();

        return ProviderName;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }
}
