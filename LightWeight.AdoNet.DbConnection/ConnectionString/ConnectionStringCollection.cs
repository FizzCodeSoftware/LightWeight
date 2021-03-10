namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Collections.Generic;
    using FizzCode.LightWeight.Configuration;
    using Microsoft.Extensions.Configuration;

    public class ConnectionStringCollection
    {
        private readonly Dictionary<string, NamedConnectionString> _connectionStrings = new Dictionary<string, NamedConnectionString>(StringComparer.InvariantCultureIgnoreCase);
        public IEnumerable<NamedConnectionString> All => _connectionStrings.Values;

        public void LoadFromConfiguration(IConfiguration configuration, string sectionKey = "ConnectionStrings", IConfigurationSecretProtector secretProtector = null)
        {
            var children = configuration
                .GetSection(sectionKey)
                .GetChildren();

            foreach (var child in children)
            {
                var name = child.Key;
                var providerName = ConfigurationReader.GetCurrentValue(configuration, child.Path, "ProviderName", null, secretProtector);
                var connectionString = ConfigurationReader.GetCurrentValue(configuration, child.Path, "ConnectionString", null, secretProtector);
                var version = ConfigurationReader.GetCurrentValue(configuration, child.Path, "Version", null, secretProtector);

                Add(new NamedConnectionString(name, providerName, connectionString, version));
            }
        }

        public void Add(NamedConnectionString connectionString)
        {
            _connectionStrings[connectionString.Name] = connectionString;
        }

        public NamedConnectionString this[string name]
        {
            get
            {
                _connectionStrings.TryGetValue(name, out var value);
                return value;
            }
        }
    }
}