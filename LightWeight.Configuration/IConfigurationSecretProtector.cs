namespace FizzCode.LightWeight.Configuration;

public interface IConfigurationSecretProtector
{
    bool Init(IConfigurationSection configurationSection);

    string Encrypt(string value);
    string Decrypt(string value);
}
