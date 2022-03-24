namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Collections.Generic;

    public static class SqlEngineSemanticFormatter
    {
        private static readonly Dictionary<string, SqlEngine> _sqlEngineByProviderName = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, ISqlEngineSemanticFormatter> _formattersByProviderName = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<SqlEngine, ISqlEngineSemanticFormatter> _formattersBySqlEngine = new();

        static SqlEngineSemanticFormatter()
        {
            RegisterFormatter(new GenericSemanticFormatter());
            RegisterFormatter(new MySqlSemanticFormatter());
            RegisterFormatter(new OracleSemanticFormatter());
            RegisterFormatter(new PostgreSqlSemanticFormatter());
            RegisterFormatter(new SqLiteSemanticFormatter());
            RegisterFormatter(new SqlServerSemanticFormatter());
            RegisterFormatter(new LegacySqlServerSemanticFormatter());
        }

        public static void RegisterFormatter(ISqlEngineSemanticFormatter formatter)
        {
            if (formatter.ProviderName != null)
            {
                _sqlEngineByProviderName[formatter.ProviderName] = formatter.SqlEngine;
                _formattersByProviderName[formatter.ProviderName] = formatter;
            }

            _formattersBySqlEngine[formatter.SqlEngine] = formatter;
        }

        public static SqlEngine GetSqlEngineByProviderName(string providerName)
        {
            if (_sqlEngineByProviderName.TryGetValue(providerName, out var sqlEngine))
                return sqlEngine;

            return SqlEngine.Generic;
        }

        public static ISqlEngineSemanticFormatter GetFormatter(NamedConnectionString connectionString)
        {
            if (_formattersByProviderName.TryGetValue(connectionString.ProviderName, out var formatter))
                return formatter;

            return _formattersBySqlEngine[SqlEngine.Generic];
        }

        public static ConnectionStringFields GetKnownConnectionStringFields(this NamedConnectionString connectionString)
        {
            return GetFormatter(connectionString).GetKnownConnectionStringFields(connectionString);
        }

        public static bool IsEscaped(this NamedConnectionString connectionString, string identifier)
        {
            return GetFormatter(connectionString).IsEscaped(identifier);
        }

        public static string Escape(this NamedConnectionString connectionString, string dbObject, string schema = null)
        {
            return GetFormatter(connectionString).Escape(dbObject, schema);
        }

        public static string Unescape(this NamedConnectionString connectionString, string identifier)
        {
            return GetFormatter(connectionString).Unescape(identifier);
        }
    }
}