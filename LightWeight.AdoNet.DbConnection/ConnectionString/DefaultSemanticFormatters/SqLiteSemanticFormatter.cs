namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Linq;

    public class SqLiteSemanticFormatter : ISqlEngineSemanticFormatter
    {
        public string ProviderName => "System.Data.SQLite";
        public SqlEngine SqlEngine => SqlEngine.SqLite;

        public string Escape(string dbObject, string schema = null)
        {
            if (!string.IsNullOrEmpty(schema))
                return EscapeIdentifier(schema) + "." + EscapeIdentifier(dbObject);

            return EscapeIdentifier(dbObject);
        }

        public string EscapeIdentifier(string identifier)
        {
            return identifier.StartsWith('\"') && identifier.EndsWith('\"')
                ? identifier
                : "\"" + identifier + "\"";
        }

        public bool IsEscaped(string identifier)
        {
            return identifier.StartsWith('\"') && identifier.EndsWith('\"');
        }

        public string Unescape(string identifier)
        {
            return identifier
                .Replace("\"", "", StringComparison.InvariantCulture);
        }

        public ConnectionStringFields GetKnownConnectionStringFields(NamedConnectionString connectionString)
        {
            if (string.IsNullOrEmpty(connectionString.ConnectionString))
                return null;

            var values = connectionString.ConnectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            var result = new ConnectionStringFields();

            foreach (var v in values)
            {
                var idx = v.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                if (idx == -1)
                    continue;

                var name = v.Substring(0, idx).ToUpperInvariant();
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
}