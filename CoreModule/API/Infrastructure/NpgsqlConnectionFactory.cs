using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    /// <summary>
    /// Factory for creating Npgsql database connections for PostgreSQL
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }

    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}

