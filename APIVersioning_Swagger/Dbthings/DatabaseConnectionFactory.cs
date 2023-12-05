using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace APIVersioning_Swagger.Dbthings
{
    public class DatabaseConnectionFactory : IDisposable
    {
        private readonly int _maxPoolSize;
        private bool disposed = false;
        private Queue<DbConnection> connectionPool = new Queue<DbConnection>();

        private readonly string DatabaseType;

        public DatabaseConnectionFactory(int maxPoolSize,string databaseType)
        {
            //_config = config;
            _maxPoolSize = maxPoolSize;
            DatabaseType = databaseType;
        }

        public DbConnection CreateConnection()
        {
            
            lock (connectionPool)
            {
                // Check if there are available connections in the pool
                if (connectionPool.Count > 0)
                {
                    var pooledConnection = connectionPool.Dequeue();
                    if (IsConnectionValid(pooledConnection))
                    {
                        return pooledConnection;
                    }
                    else
                    {
                        // If the pooled connection is no longer valid, create a new one
                        pooledConnection.Dispose();
                    }
                }

                // If the maximum pool size is not reached, create a new connection
                if (connectionPool.Count < _maxPoolSize)
                {
                    switch (DatabaseType)
                    {
                        case "SqlServer":
                            var item = new SqlConnection("Integrated Security=SSPI;Initial Catalog=Northwind");
                            connectionPool.Enqueue(item);
                            return item;
                        //case "MySql":
                        //    return new MySqlConnection(_config.ConnectionString);
                        case "PostgreSQL":
                            return new NpgsqlConnection("");
                        // Add support for other database types as needed
                        default:
                            throw new ArgumentException("Invalid database type");
                    }
                }
            }

            throw new InvalidOperationException("Connection pool limit reached");
        }

        public void ReleaseConnection(DbConnection connection)
        {
            if (IsConnectionValid(connection))
            {
                lock (connectionPool)
                {
                    if (connectionPool.Count < _maxPoolSize)
                    {
                        connectionPool.Enqueue(connection);
                    }
                    else
                    {
                        connection.Dispose();
                    }
                }
            }
            else
            {
                // If the connection is no longer valid, dispose it
                connection.Dispose();
            }
        }

        private bool IsConnectionValid(DbConnection connection)
        {
            // Add logic to check if the connection is still open and valid
            return (connection != null && connection.State == ConnectionState.Open);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of any managed resources held by the factory here.
                    foreach (var connection in connectionPool)
                    {
                        connection.Dispose();
                    }
                    connectionPool.Clear();
                }

                // Dispose of unmanaged resources here if needed.

                disposed = true;
            }
        }

        // Destructor (finalizer) - if needed
        //~DbConnectionFactory()
        //{
        //    Dispose(false);
        //}

    }
}
