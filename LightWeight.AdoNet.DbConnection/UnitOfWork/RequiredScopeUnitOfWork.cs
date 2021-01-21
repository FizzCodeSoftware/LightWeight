namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Transactions;

    public class RequiredScopeUnitOfWork : IUnitOfWork
    {
        private TransactionScope _scope;
        private bool _disposed;

        public RequiredScopeUnitOfWork()
        {
            _scope = new TransactionScope(TransactionScopeOption.Required);
        }

        public void Complete()
        {
            _scope.Complete();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _scope.Dispose();
                }

                _scope = null;
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}