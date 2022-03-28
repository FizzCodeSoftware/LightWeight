namespace FizzCode.LightWeight.AdoNet;

public interface IDatabaseContext
{
    public ConnectionManager ConnectionManager { get; }
    public CancellationToken CancellationToken { get; }
    public IUnitOfWork NewUnitOfWork();
}
