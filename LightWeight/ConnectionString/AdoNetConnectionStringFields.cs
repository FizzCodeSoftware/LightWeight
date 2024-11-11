namespace FizzCode;

public class AdoNetConnectionStringFields
{
    public string Server { get; internal set; }
    public int? Port { get; internal set; }
    public string Database { get; internal set; }
    public string UserId { get; internal set; }
    public bool? IntegratedSecurity { get; internal set; }
    public bool? Encrypt { get; internal set; }
}
