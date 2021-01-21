namespace FizzCode.LightWeight.AdoNet
{
    public class DatabaseContext : IDatabaseContext
    {
        public ConnectionManager ConnectionManager { get; } = new ConnectionManager();

        public IUnitOfWork NewUnitOfWork()
        {
            return new RequiredScopeUnitOfWork();
        }
    }
}