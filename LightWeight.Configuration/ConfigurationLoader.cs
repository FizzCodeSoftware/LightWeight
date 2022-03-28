namespace FizzCode.LightWeight.Configuration;

public static class ConfigurationLoader
{
    public static IConfigurationRoot LoadFromJsonFile(string fileName, bool optional = false)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(fileName + ".json", optional)
            .AddJsonFile(fileName + "-local.json", true)
            .AddJsonFile(fileName + "-" + Environment.MachineName + ".json", true)
            .Build();

        return configuration;
    }
}
