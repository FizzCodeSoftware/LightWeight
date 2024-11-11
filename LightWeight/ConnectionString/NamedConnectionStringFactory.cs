namespace FizzCode;

public static class NamedConnectionStringFactory
{
    public static INamedConnectionString Create(string name, string providerName, string connectionString, string version = null)
    {
        return providerName switch
        {
            AzureStorageAccountConnectionString.DefaultProviderName => new AzureStorageAccountConnectionString(name, connectionString),
            MsSqlConnectionString.DefaultProviderName => new MsSqlConnectionString(name, connectionString, version),
            MySqlConnectionString.DefaultProviderName => new MySqlConnectionString(name, connectionString, version),
            OracleSqlConnectionString.DefaultProviderName => new OracleSqlConnectionString(name, connectionString, version),
            PostgreSqlConnectionString.DefaultProviderName => new PostgreSqlConnectionString(name, connectionString, version),
            SqLiteConnectionString.DefaultProviderName => new SqLiteConnectionString(name, connectionString, version),
            _ => new GenericNamedConnectionString(name, providerName, connectionString, version)
        };
    }
}