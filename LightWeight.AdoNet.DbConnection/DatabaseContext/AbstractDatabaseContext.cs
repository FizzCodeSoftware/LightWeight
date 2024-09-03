namespace FizzCode.LightWeight.AdoNet;

public abstract class AbstractDatabaseContext : IDatabaseContext
{
    public AdoNetSqlConnectionManager ConnectionManager { get; }
    public CancellationToken CancellationToken { get; }

    protected AbstractDatabaseContext(AdoNetSqlConnectionManager connectionManager, CancellationToken cancellationToken)
    {
        ConnectionManager = connectionManager;
        CancellationToken = cancellationToken;
    }

    public abstract IUnitOfWork NewUnitOfWork();
}
