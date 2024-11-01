namespace FizzCode.LightWeight;

public class MsSqlConnectionString : IAdoNetSqlConnectionString
{
    public required string Name { get; init; }
    private string _connectionString;
#pragma warning disable RCS1085 // Use auto-implemented property
    public required string ConnectionString { get => _connectionString; init => _connectionString = value; }
#pragma warning restore RCS1085 // Use auto-implemented property
    public string Version { get; init; }

    public AdoNetEngine SqlEngine => AdoNetEngine.MsSql;
    public const string DefaultProviderName = "Microsoft.Data.SqlClient";
    public string ProviderName => DefaultProviderName;

    private static readonly Regex _regex = new(@" *(\[[^]]+\]|\w+)");

    public MsSqlConnectionString()
    {
    }

    [SetsRequiredMembers]
    public MsSqlConnectionString(string name, string connectionString, string version = null)
    {
        Name = name;
        ConnectionString = connectionString;
        Version = version;
    }

    public void SetInitialCatalog(string databaseName)
    {
        var idx = _connectionString.IndexOf("Initial Catalog", StringComparison.InvariantCulture);
        if (idx == -1)
        {
            _connectionString = _connectionString + ";Initial Catalog=" + databaseName;
        }
        else
        {
            var idx2 = _connectionString.IndexOf(';', idx + 1);
            if (idx2 == -1)
            {
                // append to the end
                _connectionString = string.Concat(
                    _connectionString.AsSpan(0, idx),
                    ";Initial Catalog=",
                    databaseName);
            }
            else
            {
                // replace
                _connectionString = string.Concat(
                    _connectionString.AsSpan(0, idx),
                    ";Initial Catalog=",
                    databaseName,
                    _connectionString.AsSpan(idx2));
            }
        }
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Name, ProviderName);
    }

    public string GetObjectIdentifier(string fullIdentifier)
    {
        if (fullIdentifier.Contains('.', StringComparison.InvariantCultureIgnoreCase))
        {
            if (fullIdentifier.Contains('[', StringComparison.InvariantCultureIgnoreCase))
            {
                var matches = _regex.Matches(fullIdentifier);
                return matches[^1].Value;
            }
            else
            {
                var groups = fullIdentifier.Split('.');
                return groups[^1];
            }
        }

        return fullIdentifier;
    }

    public string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier)
    {
        if (fullIdentifier.Contains('[', StringComparison.InvariantCultureIgnoreCase))
        {
            if (fullIdentifier.Contains('.', StringComparison.InvariantCultureIgnoreCase))
            {
                var sb = new StringBuilder();
                var matches = _regex.Matches(fullIdentifier);
                for (var i = 0; i < matches.Count - 1; i++)
                {
                    sb.Append(matches[i].Value);
                    sb.Append('.');
                }

                sb.Append(Escape(newObjectIdentifier));
                return sb.ToString();
            }
            else
            {
                return Escape(newObjectIdentifier);
            }
        }

        if (fullIdentifier.Contains('.', StringComparison.InvariantCultureIgnoreCase))
        {
            var sb = new StringBuilder();
            var groups = fullIdentifier.Split('.');
            for (var i = 0; i < groups.Length - 1; i++)
            {
                sb.Append(groups[i]);
                sb.Append('.');
            }

            if (IsEscaped(groups[^1]))
                sb.Append(Escape(newObjectIdentifier));
            else
                sb.Append(newObjectIdentifier);

            return sb.ToString();
        }
        else
        {
            return newObjectIdentifier;
        }
    }

    public string Escape(string dbObject, string schema = null)
    {
        if (!string.IsNullOrEmpty(schema))
            return EscapeIdentifier(schema) + "." + EscapeIdentifier(dbObject);

        return EscapeIdentifier(dbObject);
    }

    public string EscapeIdentifier(string identifier)
    {
        return identifier.StartsWith('[') && identifier.EndsWith(']')
             ? identifier
             : "[" + identifier + "]";
    }

    public bool IsEscaped(string identifier)
    {
        return identifier.StartsWith('[') && identifier.EndsWith(']');
    }

    public string Unescape(string identifier)
    {
        return identifier
            .Replace("[", "", StringComparison.InvariantCulture)
            .Replace("]", "", StringComparison.InvariantCulture);
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
                case "ENCRYPT":
                    result.Encrypt = string.Equals(value, "yes", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(value, "1", StringComparison.InvariantCultureIgnoreCase);
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