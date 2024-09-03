namespace FizzCode.LightWeight;

public interface INamedConnectionString
{
    public string Name { get; }
    public string ConnectionString { get; }
    public string ProviderName { get; }
    public string Version { get; }
}