namespace FizzCode.LightWeight.AdoNet;

public interface IDatabaseContext
{
    public AdoNetSqlConnectionManager ConnectionManager { get; }
    public CancellationToken CancellationToken { get; }
    public IUnitOfWork NewUnitOfWork();
}
