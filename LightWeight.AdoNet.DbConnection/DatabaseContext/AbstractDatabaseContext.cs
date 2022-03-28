namespace FizzCode.LightWeight.AdoNet;

public abstract class AbstractDatabaseContext : IDatabaseContext
{
    public ConnectionManager ConnectionManager { get; }
    public CancellationToken CancellationToken { get; }

    protected AbstractDatabaseContext(ConnectionManager connectionManager, CancellationToken cancellationToken)
    {
        ConnectionManager = connectionManager;
        CancellationToken = cancellationToken;
    }

    public abstract IUnitOfWork NewUnitOfWork();
}
