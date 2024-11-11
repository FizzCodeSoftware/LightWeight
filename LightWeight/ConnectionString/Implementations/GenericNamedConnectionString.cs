namespace FizzCode;

[DebuggerDisplay("{Name}, {ProviderName}, {ConnectionString}")]
public class GenericNamedConnectionString : INamedConnectionString
{
    public required string Name { get; init; }
    public required string ProviderName { get; init; }
    public required string ConnectionString { get; init; }
    public string Version { get; init; }

    public GenericNamedConnectionString()
    {
    }

    [SetsRequiredMembers]
    public GenericNamedConnectionString(string name, string providerName, string connectionString, string version = null)
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