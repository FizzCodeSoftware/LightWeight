namespace FizzCode.LightWeight.AdoNet;

public class DatabaseConnection
{
    public ConnectionManager Manager { get; init; }
    public string Key { get; init; }
    public NamedConnectionString ConnectionString { get; init; }
    public IDbConnection Connection { get; init; }
    public Transaction TransactionWhenCreated { get; set; }
    public object Lock { get; set; } = new object();

    public int ReferenceCount { get; internal set; }
    public bool Failed { get; internal set; }

    public void SetFailed()
    {
        Manager?.ConnectionFailed(this);
    }
}
