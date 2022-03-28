namespace FizzCode.LightWeight.AdoNet;

public class DisposableDatabaseConnection : DatabaseConnection, IDisposable
{
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Manager.ReleaseConnection(this);
        }

        if (ReferenceCount == 0)
        {
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
