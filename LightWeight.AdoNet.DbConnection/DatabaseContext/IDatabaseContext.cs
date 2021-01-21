namespace FizzCode.LightWeight.AdoNet
{
    public interface IDatabaseContext
    {
        ConnectionManager ConnectionManager { get; }
        IUnitOfWork NewUnitOfWork();
    }
}