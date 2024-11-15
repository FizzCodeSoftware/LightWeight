namespace FizzCode.LightWeight.AdoNet;

public delegate void OnAdoNetSqlConnectionOpening(IAdoNetSqlConnectionString connectionString, IDbConnection connection);
public delegate void OnAdoNetSqlConnectionOpened(IAdoNetSqlConnectionString connectionString, IDbConnection connection, int retryCount);
public delegate void OnAdoNetSqlConnectionOpenError(IAdoNetSqlConnectionString connectionString, IDbConnection connection, int retryCount, Exception ex);

public delegate void OnAdoNetSqlConnectionClosing(DatabaseConnection connection);
public delegate void OnAdoNetSqlConnectionClosed(DatabaseConnection connection);
public delegate void OnAdoNetSqlConnectionCloseError(DatabaseConnection connection, Exception ex);

public class AdoNetSqlConnectionManager
{
    public bool SeparateConnectionsByThreadId { get; set; } = true;

    private readonly Dictionary<string, DatabaseConnection> _connections = [];

    public DisposableDatabaseConnection GetDisposableConnection(IAdoNetSqlConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnAdoNetSqlConnectionOpening onOpening = null, OnAdoNetSqlConnectionOpened onOpened = null, OnAdoNetSqlConnectionOpenError onError = null)
    {
        return GetConnection<DisposableDatabaseConnection>(connectionString, maxRetryCount, retryDelayMilliseconds, onOpening, onOpened, onError);
    }

    public DatabaseConnection GetConnection(IAdoNetSqlConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnAdoNetSqlConnectionOpening onOpening = null, OnAdoNetSqlConnectionOpened onOpened = null, OnAdoNetSqlConnectionOpenError onError = null)
    {
        return GetConnection<DatabaseConnection>(connectionString, maxRetryCount, retryDelayMilliseconds, onOpening, onOpened, onError);
    }

    public T GetConnection<T>(IAdoNetSqlConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnAdoNetSqlConnectionOpening onOpening = null, OnAdoNetSqlConnectionOpened onOpened = null, OnAdoNetSqlConnectionOpenError onError = null)
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
            key += Environment.CurrentManagedThreadId.ToString("D", CultureInfo.InvariantCulture);
        }
        else
        {
            key += "-";
        }

        List<Exception> exceptions = null;

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
                        try
                        {
                            var factory = DbProviderFactories.GetFactory(connectionString.ProviderName)
                                ?? throw new Exception("unregistered DbProviderFactory: " + connectionString.ProviderName);

                            conn = factory.CreateConnection();
                        }
                        catch (Exception ex)
                        {
                            onError?.Invoke(connectionString, conn, retry, ex);
                            throw;
                        }
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
                    (exceptions ??= []).Add(ex);
                }
            } // lock released

            if (retry < maxRetryCount)
            {
                Thread.Sleep(retryDelayMilliseconds * (retry + 1));
            }
        }

        if (exceptions != null)
        {
            var aggEx = new AggregateException(exceptions);
            onError?.Invoke(connectionString, null, maxRetryCount, aggEx);
            throw aggEx;
        }

        return null;
    }

    public DatabaseConnection GetNewConnection(IAdoNetSqlConnectionString connectionString, int maxRetryCount = 5, int retryDelayMilliseconds = 2000, OnAdoNetSqlConnectionOpening onOpening = null, OnAdoNetSqlConnectionOpened onOpened = null, OnAdoNetSqlConnectionOpenError onError = null)
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
                    try
                    {
                        var factory = DbProviderFactories.GetFactory(connectionString.ProviderName)
                            ?? throw new Exception("unregistered DbProviderFactory: " + connectionString.ProviderName);

                        conn = factory.CreateConnection();
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(connectionString, conn, retry, ex);
                        throw;
                    }
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
                return transaction.TransactionInformation.LocalIdentifier[^10..] + "::" + transaction.TransactionInformation.DistributedIdentifier.ToString("N", CultureInfo.InvariantCulture)[26..];

            return transaction.TransactionInformation.LocalIdentifier[^10..];
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

    public void ReleaseConnection(DatabaseConnection connection, OnAdoNetSqlConnectionClosing onClosing = null, OnAdoNetSqlConnectionClosed onClosed = null, OnAdoNetSqlConnectionCloseError onError = null)
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

    public void TestConnection(IAdoNetSqlConnectionString connectionString)
    {
        IDbConnection conn = null;

        var connectionType = Type.GetType(connectionString.ProviderName);
        if (connectionType != null)
        {
            conn = Activator.CreateInstance(connectionType) as IDbConnection;
        }

        conn ??= DbProviderFactories.GetFactory(connectionString.ProviderName).CreateConnection();

        conn.ConnectionString = connectionString.ConnectionString;
        conn.Open();

        conn.Close();
        conn.Dispose();
    }
}
