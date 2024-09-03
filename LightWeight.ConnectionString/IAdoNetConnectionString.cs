﻿namespace FizzCode.LightWeight;

public interface IAdoNetConnectionString : INamedConnectionString
{
    public AdoNetEngine SqlEngine { get; }

    public bool IsEscaped(string identifier);
    public string Escape(string dbObject, string schema = null);
    string EscapeIdentifier(string identifier);
    public string Unescape(string identifier);

    string ChangeObjectIdentifier(string fullIdentifier, string newObjectIdentifier);
    string GetObjectIdentifier(string fullIdentifier);

    public string GetFriendlyProviderName() => SqlEngine.ToString() ?? ProviderName;
    public AdoNetConnectionStringFields GetFields();
}