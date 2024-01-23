namespace FizzCode.LightWeight.AdoNet;

public class AdoNetConnectionStringFields
{
    public string Server { get; set; }
    public int? Port { get; set; }
    public string Database { get; set; }
    public string UserId { get; set; }
    public bool? IntegratedSecurity { get; set; }
    public bool? Encrypt { get; set; }
}
