namespace FizzCode;

public class OracleSqlConnectionString(string name, string connectionString, string version = null)
    : BaseAdoNetSqlConnectionString(AdoNetEngine.OracleSql, DefaultProviderName, name, connectionString, version)
{
    public const string DefaultProviderName = "Oracle.ManagedDataAccess.Client";

    public override string GetObjectIdentifier(string fullIdentifier)
    {
        return fullIdentifier;
    }

    public override string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier)
    {
        return newObjectIdentifier;
    }

    public override string Escape(string dbObject, string schema = null)
    {
        if (!string.IsNullOrEmpty(schema))
            return EscapeIdentifier(schema) + "." + EscapeIdentifier(dbObject);

        return EscapeIdentifier(dbObject);
    }

    public override string EscapeIdentifier(string identifier)
    {
        return identifier.StartsWith('\"') && identifier.EndsWith('\"')
            ? identifier
            : "\"" + identifier + "\"";
    }

    public override bool IsEscaped(string identifier)
    {
        return identifier.StartsWith('\"') && identifier.EndsWith('\"');
    }

    public override string Unescape(string identifier)
    {
        return identifier
            .Replace("\"", "", StringComparison.InvariantCulture);
    }

    protected override AdoNetConnectionStringFields GetFields()
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