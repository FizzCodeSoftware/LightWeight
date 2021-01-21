namespace FizzCode.LightWeight.AdoNet
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Threading;
    using System.Transactions;

    public delegate void OnConnectionOpening(NamedConnectionString connectionString, IDbConnection connection);
    public delegate void OnConnectionOpened(NamedConnectionString connectionString, IDbConnection connection, int retryCount);
    public delegate void OnConnectionOpenError(NamedConnectionString connectionString, IDbConnection connection, int retryCount, Exception ex);

    public delegate void OnConnectionClosing(DatabaseConnection connection);
    public delegate void OnConnectionClosed(DatabaseConnection connection);
    public delegate void OnConnectionCloseError(DatabaseConnection connection, Exception ex);

    public class ConnectionManager
    {
        public bool SeparateConnectionsByThreadId { get; set; } = true;

        private readonly Dictionary<string, DatabaseConnection> _connections = new Dictionary<string, DatabaseConnection>();

        public DisposableDatabaseConnection GetDisposableConnection(NamedConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000)
        {
            return GetConnection<DisposableDatabaseConnection>(connectionString, maxRetryCount, retryDelayMilliseconds);
        }

        public DatabaseConnection GetConnection(NamedConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnConnectionOpening onOpening = null, OnConnectionOpened onOpened = null, OnConnectionOpenError onError = null)
        {
            return GetConnection<DatabaseConnection>(connectionString, maxRetryCount, retryDelayMilliseconds, onOpening, onOpened, onError);
        }

        public T GetConnection<T>(NamedConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnConnectionOpening onOpening = null, OnConnectionOpened onOpened = null, OnConnectionOpenError onError = null)
            where T : DatabaseConnection, new()
        {
            var key = connectionString.Name;

            if (Transaction.Current != null)
            {
                key += GetTransactionIdentifierString(Transaction.Current);
            }
            else
            {
                key += "-";
            }

            if (SeparateConnectionsByThreadId)
            {
                key += Thread.CurrentThread.ManagedThreadId.ToString("D", CultureInfo.InvariantCulture);
            }
            else
            {
                key += "-";
            }

            Exception lastException = null;

            for (var retry = 0; retry <= maxRetryCount; retry++)
            {
                lock (_connections)
                {
                    if (_connections.TryGetValue(key, out var connection))
                    {
                        connection.ReferenceCount++;
                        return connection as T;
                    }

                    try
                    {
                        IDbConnection conn = null;

                        var connectionType = Type.GetType(connectionString.ProviderName);
                        if (connectionType != null)
                        {
                            conn = Activator.CreateInstance(connectionType) as IDbConnection;
                        }

                        if (conn == null)
                        {
                            conn = DbProviderFactories.GetFactory(connectionString.ProviderName).CreateConnection();
                        }

                        conn.ConnectionString = connectionString.ConnectionString;

                        onOpening?.Invoke(connectionString, conn);

                        try
                        {
                            conn.Open();
                            onOpened?.Invoke(connectionString, conn, retry);
                        }
                        catch (Exception ex)
                        {
                            onError?.Invoke(connectionString, conn, retry, ex);
                            throw;
                        }

                        return new T()
                        {
                            Manager = this,
                            Key = null,
                            ConnectionString = connectionString,
                            Connection = conn,
                            ReferenceCount = 1,
                            TransactionWhenCreated = Transaction.Current,
                        };
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                } // lock released

                if (retry < maxRetryCount)
                {
                    Thread.Sleep(retryDelayMilliseconds * (retry + 1));
                }
            }

            return null;
        }

        public DatabaseConnection GetNewConnection(NamedConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnConnectionOpening onOpening = null, OnConnectionOpened onOpened = null, OnConnectionOpenError onError = null)
        {
            Exception lastException = null;

            for (var retry = 0; retry <= maxRetryCount; retry++)
            {
                try
                {
                    IDbConnection conn = null;

                    var connectionType = Type.GetType(connectionString.ProviderName);
                    if (connectionType != null)
                    {
                        conn = Activator.CreateInstance(connectionType) as IDbConnection;
                    }

                    if (conn == null)
                    {
                        conn = DbProviderFactories.GetFactory(connectionString.ProviderName).CreateConnection();
                    }

                    conn.ConnectionString = connectionString.ConnectionString;

                    onOpening?.Invoke(connectionString, conn);

                    try
                    {
                        conn.Open();
                        onOpened?.Invoke(connectionString, conn, retry);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(connectionString, conn, retry, ex);
                        throw;
                    }

                    return new DatabaseConnection()
                    {
                        Manager = this,
                        Key = null,
                        ConnectionString = connectionString,
                        Connection = conn,
                        ReferenceCount = 1,
                        TransactionWhenCreated = Transaction.Current,
                    };
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (retry < maxRetryCount)
                {
                    Thread.Sleep(retryDelayMilliseconds * (retry + 1));
                }
            }

            return null;
        }

        private string GetTransactionIdentifierString(Transaction transaction)
        {
            if (transaction == null)
                return null;

            if (transaction.TransactionInformation.LocalIdentifier != null)
            {
                if (transaction.TransactionInformation.DistributedIdentifier != Guid.Empty)
                    return transaction.TransactionInformation.LocalIdentifier.Substring(transaction.TransactionInformation.LocalIdentifier.Length - 10) + "::" + transaction.TransactionInformation.DistributedIdentifier.ToString("N", CultureInfo.InvariantCulture).Substring(26);

                return transaction.TransactionInformation.LocalIdentifier.Substring(transaction.TransactionInformation.LocalIdentifier.Length - 10);
            }

            return transaction.TransactionInformation
                    .CreationTime
                    .ToString("HHmmssfff", CultureInfo.InvariantCulture);
        }

        public void ConnectionFailed(DatabaseConnection connection)
        {
            lock (_connections)
            {
                connection.ReferenceCount--;
                connection.Failed = true;

                if (connection.Key != null)
                {
                    _connections.Remove(connection.Key);
                }

                if (connection.ReferenceCount == 0 && connection != null)
                {
                    connection.Connection.Close();
                    connection.Connection.Dispose();
                }
            }
        }

        public void ReleaseConnection(DatabaseConnection connection, OnConnectionClosing onClosing = null, OnConnectionClosed onClosed = null, OnConnectionCloseError onError = null)
        {
            if (connection == null)
                return;

            lock (_connections)
            {
                connection.ReferenceCount--;

                if (connection.ReferenceCount == 0)
                {
                    if (connection.Key != null)
                    {
                        _connections.Remove(connection.Key);
                    }

                    onClosing?.Invoke(connection);

                    try
                    {
                        connection.Connection.Close();
                        connection.Connection.Dispose();
                        onClosed?.Invoke(connection);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(connection, ex);
                    }
                }
            }
        }

        public static void TestConnection(NamedConnectionString connectionString)
        {
            IDbConnection conn = null;

            var connectionType = Type.GetType(connectionString.ProviderName);
            if (connectionType != null)
            {
                conn = Activator.CreateInstance(connectionType) as IDbConnection;
            }

            if (conn == null)
            {
                conn = DbProviderFactories.GetFactory(connectionString.ProviderName).CreateConnection();
            }

            conn.ConnectionString = connectionString.ConnectionString;
            conn.Open();

            conn.Close();
            conn.Dispose();
        }
    }
}