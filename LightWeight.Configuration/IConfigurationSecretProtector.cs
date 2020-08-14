namespace FizzCode.LightWeight.Configuration
{
    using Microsoft.Extensions.Configuration;

    public interface IConfigurationSecretProtector
    {
        bool Init(IConfigurationSection configurationSection);

        string Encrypt(string value);
        string Decrypt(string value);
    }
}