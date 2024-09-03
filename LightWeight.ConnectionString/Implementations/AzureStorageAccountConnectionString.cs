namespace FizzCode.LightWeight;

[EditorBrowsable(EditorBrowsableState.Never)]
public class AzureStorageAccountConnectionString : INamedConnectionString
{
    public required string Name { get; init; }
    public required string ConnectionString { get; init; }

    public string AccountName => _accountName ??= ConnectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(x => x.StartsWith("AccountName=", StringComparison.InvariantCultureIgnoreCase) && x.Length > "AccountName=".Length)
        .Select(x => x.Split('=')[1].Trim())
        .FirstOrDefault() ?? "";

    public const string DefaultProviderName = "AzureStorageAccount";

    public string ProviderName => DefaultProviderName;
    public string Version => null;

    private string _accountName;

    public AzureStorageAccountConnectionString()
    {
    }

    [SetsRequiredMembers]
    public AzureStorageAccountConnectionString(string name, string connectionString)
    {
        Name = name;
        ConnectionString = connectionString;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }
}