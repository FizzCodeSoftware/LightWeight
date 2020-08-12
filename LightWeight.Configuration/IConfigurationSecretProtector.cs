namespace FizzCode.LightWeight.Configuration
{
    public interface IConfigurationSecretProtector
    {
        string Encrypt(string value);
        string Decrypt(string value);
    }
}