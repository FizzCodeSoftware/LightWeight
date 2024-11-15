namespace FizzCode;

public class AzureStorageAccountConnectionString : INamedConnectionString
{
    public string Name { get; }
    public string ConnectionString { get; }

    public string AccountName { get; }

    public const string DefaultProviderName = "AzureStorageAccount";

    public string ProviderName => DefaultProviderName;
    public string Version => null;

    public AzureStorageAccountConnectionString(string name, string connectionString)
    {
        Name = name;
        ConnectionString = connectionString;
        AccountName = ConnectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.StartsWith("AccountName=", StringComparison.InvariantCultureIgnoreCase) && x.Length > "AccountName=".Length)
            .Select(x => x.Split('=')[1].Trim())
            .FirstOrDefault() ?? "";
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}, {2}", Name, AccountName, ProviderName);
    }
}