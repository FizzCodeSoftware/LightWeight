namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NamedConnectionString
    {
        public string Name { get; set; }
        public string ProviderName { get => _providerName; set { _providerName = value; UpdateEngine(); } }
        public string ConnectionString { get; set; }
        public SqlEngine? SqlEngine { get; private set; }

        private string _providerName;

        public static Dictionary<string, SqlEngine> SqlEngineByProviderNameMap { get; } = new Dictionary<string, SqlEngine>()
        {
            ["System.Data.SqlClient"] = AdoNet.SqlEngine.MsSql,
            ["MySql.Data.MySqlClient"] = AdoNet.SqlEngine.MySql,
            ["Oracle.ManagedDataAccess.Client"] = AdoNet.SqlEngine.OracleSql,
            ["Npgsql"] = AdoNet.SqlEngine.PostgreSql,
            ["System.Data.SQLite"] = AdoNet.SqlEngine.SqLite,
        };

        public NamedConnectionString(string name, string providerName, string connectionString, string version)
        {
            Name = name;
            _providerName = providerName;
            ConnectionString = connectionString;
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            UpdateEngine();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void UpdateEngine()
        {
            SqlEngine = SqlEngineByProviderNameMap.TryGetValue(_providerName, out var sqlEngine)
                ? sqlEngine
                : AdoNet.SqlEngine.Generic;
        }

        public string GetFriendlyProviderName()
        {
            if (SqlEngine != null)
                return SqlEngine.ToString();

            return ProviderName;
        }

        public ConnectionStringFields GetKnownConnectionStringFields()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return null;

            var values = ConnectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            var result = new ConnectionStringFields();

            foreach (var v in values)
            {
                var idx = v.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                if (idx == -1)
                    continue;

                var name = v.Substring(0, idx).ToUpperInvariant();
                var value = v.Substring(idx + 1);
                switch (name)
                {
                    case "SERVER":
                    case "DATA SOURCE":
                        result.Server = value;
                        // todo: support oracle's complex Data Source format:
                        /*if (KnownProvider == Configuration.KnownProvider.Oracle)
                        {
                            // Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));
                        }*/
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

        public bool IsEscaped(string identifier)
        {
            if (SqlEngine == null)
                throw new NotSupportedException();

            switch (SqlEngine)
            {
                case AdoNet.SqlEngine.MsSql:
                    return identifier.StartsWith('[') && identifier.EndsWith(']');
                case AdoNet.SqlEngine.SqLite:
                case AdoNet.SqlEngine.PostgreSql:
                case AdoNet.SqlEngine.OracleSql:
                    return identifier.StartsWith('\"') && identifier.EndsWith('\"');
                case AdoNet.SqlEngine.MySql:
                    return identifier.StartsWith('`') && identifier.EndsWith('`');
            }

            throw new NotSupportedException();
        }

        public string Escape(string dbObject, string schema = null)
        {
            if (SqlEngine == null)
                throw new NotSupportedException();

            switch (SqlEngine)
            {
                case AdoNet.SqlEngine.MsSql:
                case AdoNet.SqlEngine.SqLite:
                case AdoNet.SqlEngine.MySql:
                case AdoNet.SqlEngine.PostgreSql:
                case AdoNet.SqlEngine.OracleSql:
                    if (!string.IsNullOrEmpty(schema))
                        return EscapeIdentifier(schema) + "." + EscapeIdentifier(dbObject);

                    return EscapeIdentifier(dbObject);
            }

            throw new NotSupportedException();
        }

        private string EscapeIdentifier(string identifier)
        {
            switch (SqlEngine)
            {
                case AdoNet.SqlEngine.MsSql:
                    return identifier.StartsWith('[') && identifier.EndsWith(']')
                         ? identifier
                         : "[" + identifier + "]";
                case AdoNet.SqlEngine.SqLite:
                case AdoNet.SqlEngine.PostgreSql:
                case AdoNet.SqlEngine.OracleSql:
                    return identifier.StartsWith('\"') && identifier.EndsWith('\"')
                        ? identifier
                        : "\"" + identifier + "\"";
                case AdoNet.SqlEngine.MySql:
                    return identifier.StartsWith('`') && identifier.EndsWith('`')
                        ? identifier
                        : "`" + identifier + "`";
            }

            throw new NotSupportedException();
        }

        public string Unescape(string identifier)
        {
            if (SqlEngine == null)
                throw new NotSupportedException();

            switch (SqlEngine)
            {
                case AdoNet.SqlEngine.MsSql:
                    return identifier
                        .Replace("[", "", StringComparison.InvariantCulture)
                        .Replace("]", "", StringComparison.InvariantCulture);
                case AdoNet.SqlEngine.SqLite:
                case AdoNet.SqlEngine.PostgreSql:
                case AdoNet.SqlEngine.OracleSql:
                    return identifier
                        .Replace("\"", "", StringComparison.InvariantCulture);
                case AdoNet.SqlEngine.MySql:
                    return identifier
                        .Replace("`", "", StringComparison.InvariantCulture);
            }

            throw new NotSupportedException();
        }
    }
}