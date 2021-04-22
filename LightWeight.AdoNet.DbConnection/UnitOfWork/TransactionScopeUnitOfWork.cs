namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Transactions;

    public class TransactionScopeUnitOfWork : IUnitOfWork
    {
        private TransactionScope _scope;
        private bool _disposed;

        public TransactionScopeUnitOfWork(TransactionScopeOption scopeOption = TransactionScopeOption.Required, TransactionScopeAsyncFlowOption asyncFlowOption = TransactionScopeAsyncFlowOption.Suppress, TransactionOptions? transactionOptions = null)
        {
            _scope = transactionOptions != null
                ? new TransactionScope(scopeOption, transactionOptions.Value, asyncFlowOption)
                : new TransactionScope(scopeOption, asyncFlowOption);
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