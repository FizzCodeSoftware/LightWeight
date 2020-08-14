namespace FizzCode.LightWeight.Configuration
{
    using Microsoft.Extensions.Configuration;

    public interface IConfigurationSecretProtector
    {
        void Init(IConfigurationSection configurationSection);

        string Encrypt(string value);
        string Decrypt(string value);
    }
}