namespace FizzCode.LightWeight.AdoNet;

public class ConnectionStringCollection
{
    private readonly Dictionary<string, INamedConnectionString> _connectionStrings = new(StringComparer.InvariantCultureIgnoreCase);
    public IEnumerable<INamedConnectionString> All => _connectionStrings.Values;

    public void LoadFromConfiguration(IConfiguration configuration, string sectionKey = "ConnectionStrings", IConfigurationSecretProtector secretProtector = null)
    {
        var children = configuration
            .GetSection(sectionKey)
            .GetChildren();

        foreach (var child in children)
        {
            var name = child.Key;
            var providerName = ConfigurationReader.GetCurrentValue(configuration, child.Path, "ProviderName", null, secretProtector);
            var connectionString = ConfigurationReader.GetCurrentValue(configuration, child.Path, "ConnectionString", null, secretProtector);
            var version = ConfigurationReader.GetCurrentValue(configuration, child.Path, "Version", null, secretProtector);
            var namedConnectionString = NamedConnectionStringFactory.Create(name, providerName, connectionString, version);
            Add(namedConnectionString);
        }
    }

    public void Add(INamedConnectionString connectionString)
    {
        _connectionStrings[connectionString.Name] = connectionString;
    }

    public INamedConnectionString this[string name]
    {
        get
        {
            _connectionStrings.TryGetValue(name, out var value);
            return value;
        }
    }
}
