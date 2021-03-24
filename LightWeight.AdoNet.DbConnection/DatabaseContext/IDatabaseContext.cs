namespace FizzCode.LightWeight.AdoNet
{
    using System.Threading;

    public interface IDatabaseContext
    {
        public ConnectionManager ConnectionManager { get; }
        public CancellationToken CancellationToken { get; }
        public IUnitOfWork NewUnitOfWork();
    }
}