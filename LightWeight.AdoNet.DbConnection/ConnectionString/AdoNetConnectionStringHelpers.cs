namespace FizzCode.LightWeight.AdoNet;

public static class AdoNetConnectionStringHelpers
{
    private static readonly Dictionary<string, IAdoNetConnectionStringHelper> _formattersByProviderName = new(StringComparer.InvariantCultureIgnoreCase);
    private static readonly IAdoNetConnectionStringHelper _genericFormatter = new GenericAdoNetConnectionStringHelper();

    static AdoNetConnectionStringHelpers()
    {
        RegisterHelper(new MySqlAdoNetConnectionStringHelper());
        RegisterHelper(new OracleAdoNetConnectionStringHelper());
        RegisterHelper(new PostgreSqlAdoNetConnectionStringHelper());
        RegisterHelper(new SqLiteAdoNetConnectionStringHelper());
        RegisterHelper(new SqlServerAdoNetConnectionStringHelper());
        RegisterHelper(new LegacySqlServerAdoNetConnectionStringHelper());
    }

    public static void RegisterHelper(IAdoNetConnectionStringHelper helper)
    {
        if (helper.ProviderName != null)
        {
            _formattersByProviderName[helper.ProviderName] = helper;
        }
    }

    public static IAdoNetConnectionStringHelper GetAdoNetHelper(this NamedConnectionString connectionString)
    {
        if (_formattersByProviderName.TryGetValue(connectionString.ProviderName, out var formatter))
            return formatter;

        return _genericFormatter;
    }
}