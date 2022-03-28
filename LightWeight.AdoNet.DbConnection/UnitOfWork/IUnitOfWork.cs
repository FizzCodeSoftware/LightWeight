namespace FizzCode.LightWeight.AdoNet;

public interface IUnitOfWork : IDisposable
{
    void Complete();
}
