namespace FizzCode.LightWeight.RelationalModel;

[DebuggerDisplay("{Key} = {Value}")]
public class AdditionalData
{
    public string Key { get; }
    public object Value { get; }

    internal AdditionalData(string key, object value)
    {
        Key = key;
        Value = value;
    }
}
