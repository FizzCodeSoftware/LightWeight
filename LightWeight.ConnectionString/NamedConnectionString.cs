namespace FizzCode.LightWeight;

[DebuggerDisplay("{Name}, {ProviderName}, {ConnectionString}")]
public class NamedConnectionString
{
    public required string Name { get; init; }
    public required string ProviderName { get; init; }
    public required string ConnectionString { get; init; }
    public required string Version { get; init; }

    public NamedConnectionString()
    {
    }

    [SetsRequiredMembers]
    public NamedConnectionString(string name, string providerName, string connectionString, string version)
    {
        Name = name;
        ProviderName = providerName;
        ConnectionString = connectionString;
        Version = version;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }
}