namespace FizzCode.LightWeight.Configuration
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationReader
    {
        public static T GetCurrentValue<T>(IConfiguration configuration, string section, string key, T defaultValue)
        {
            var value = configuration.GetValue<T>(section + ":" + key + "-" + Environment.MachineName, default);
            if (value != null && !value.Equals(default(T)))
                return value;

            return configuration.GetValue(section + ":" + key, defaultValue);
        }

        public static string GetCurrentValue(IConfiguration configuration, string section, string key, string defaultValue, IConfigurationSecretProtector protector = null)
        {
            var isProtected = configuration.GetValue(section + ":" + key + "-protected", false);

            var value = configuration.GetValue<string>(section + ":" + key + "-" + Environment.MachineName, default);
            if (value != null)
            {
                if (isProtected && protector != null)
                {
                    value = protector.Decrypt(value);
                }

                return value;
            }

            value = configuration.GetValue(section + ":" + key, defaultValue);
            if (value != null && isProtected && protector != null)
            {
                value = protector.Decrypt(value);
            }

            return value;
        }

        public static T GetCurrentValue<T>(IConfigurationSection section, string key, T defaultValue)
        {
            var value = section.GetValue<T>(key + "-" + Environment.MachineName, default);
            if (value != null && !value.Equals(default(T)))
                return value;

            return section.GetValue(key, defaultValue);
        }

        public static string GetCurrentValue(IConfigurationSection section, string key, string defaultValue, IConfigurationSecretProtector protector = null)
        {
            var isProtected = section.GetValue(key + "-protected", false);

            var value = section.GetValue<string>(key + "-" + Environment.MachineName, default);
            if (value != null)
            {
                if (isProtected && protector != null)
                {
                    value = protector.Decrypt(value);
                }

                return value;
            }

            value = section.GetValue(key, defaultValue);
            if (value != null && isProtected && protector != null)
            {
                value = protector.Decrypt(value);
            }

            return value;
        }
    }
}
