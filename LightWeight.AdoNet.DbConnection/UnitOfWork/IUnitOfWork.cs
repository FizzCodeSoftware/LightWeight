namespace FizzCode.LightWeight.AdoNet;

using System;

public interface IUnitOfWork : IDisposable
{
    void Complete();
}
