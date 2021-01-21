namespace FizzCode.LightWeight.AdoNet
{
    using System;

    public class DisposableDatabaseConnection : IDisposable
    {
        private bool _disposed;
        private DatabaseConnection _connection;

        internal DisposableDatabaseConnection(DatabaseConnection connection)
        {
            _connection = connection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Manager.ReleaseConnection(_connection);
                }
            }

            if (_connection.ReferenceCount == 0)
            {
                _disposed = true;
                _connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}