namespace FizzCode.LightWeight;

public class MySqlConnectionString : IAdoNetConnectionString
{
    public required string Name { get; init; }
    public required string ConnectionString { get; init; }
    public string Version { get; init; }

    public AdoNetEngine SqlEngine => AdoNetEngine.MySql;
    public const string DefaultProviderName = "MySql.Data.MySqlClient";
    public string ProviderName => DefaultProviderName;

    public MySqlConnectionString()
    {
    }

    [SetsRequiredMembers]
    public MySqlConnectionString(string name, string connectionString, string version = null)
    {
        Name = name;
        ConnectionString = connectionString;
        Version = version;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }

    public string GetObjectIdentifier(string fullIdentifier)
    {
        return fullIdentifier;
    }

    public string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier)
    {
        return newObjectIdentifier;
    }

    public string Escape(string dbObject, string schema = null)
    {
        if (!string.IsNullOrEmpty(schema))
            return EscapeIdentifier(schema) + "." + EscapeIdentifier(dbObject);

        return EscapeIdentifier(dbObject);
    }

    public string EscapeIdentifier(string identifier)
    {
        return identifier.StartsWith('`') && identifier.EndsWith('`')
            ? identifier
            : "`" + identifier + "`";
    }

    public bool IsEscaped(string identifier)
    {
        return identifier.StartsWith('`') && identifier.EndsWith('`');
    }

    public string Unescape(string identifier)
    {
        return identifier
            .Replace("`", "", StringComparison.InvariantCulture);
    }

    public AdoNetConnectionStringFields GetFields()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            return null;

        var values = ConnectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim());

        var result = new AdoNetConnectionStringFields();

        foreach (var v in values)
        {
            var idx = v.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (idx == -1)
                continue;

            var name = v[..idx].ToUpperInvariant();
            var value = v[(idx + 1)..];
            switch (name)
            {
                case "SERVER":
                case "DATA SOURCE":
                    result.Server = value;
                    break;
                case "DATABASE":
                case "INITIAL CATALOG":
                    result.Database = value;
                    break;
                case "USER ID":
                case "UID":
                    result.UserId = value;
                    break;
                case "PORT":
                    if (int.TryParse(value, out var port))
                        result.Port = port;
                    break;
                case "INTEGRATEDSECURITY":
                case "INTEGRATED SECURITY":
                    result.IntegratedSecurity = string.Equals(value, "yes", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(value, "sspi", StringComparison.InvariantCultureIgnoreCase);
                    break;
            }
        }

        return result;
    }
}