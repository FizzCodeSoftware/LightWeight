namespace FizzCode.LightWeight.AdoNet;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SqlServerAdoNetConnectionStringHelper : IAdoNetConnectionStringHelper
{
    public string ProviderName => "Microsoft.Data.SqlClient";
    public AdoNetEngine Engine => AdoNetEngine.MsSql;

    private static readonly Regex _regex = new(@" *(\[[^]]+\]|\w+)");

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

    public AdoNetConnectionStringFields GetKnownConnectionStringFields(NamedConnectionString connectionString)
    {
        if (string.IsNullOrEmpty(connectionString.ConnectionString))
            return null;

        var values = connectionString.ConnectionString
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
