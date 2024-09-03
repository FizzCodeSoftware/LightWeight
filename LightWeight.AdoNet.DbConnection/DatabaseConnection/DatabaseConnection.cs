namespace FizzCode.LightWeight.AdoNet;

public class DatabaseConnection
{
    public AdoNetSqlConnectionManager Manager { get; internal init; }
    public string Key { get; internal init; }
    public IAdoNetSqlConnectionString ConnectionString { get; internal init; }
    public IDbConnection Connection { get; internal init; }
    public Transaction TransactionWhenCreated { get; internal init; }
    public object Lock { get; } = new object();

    public int ReferenceCount { get; internal set; }
    public bool Failed { get; internal set; }

    public void SetFailed()
    {
        Manager?.ConnectionFailed(this);
    }
}