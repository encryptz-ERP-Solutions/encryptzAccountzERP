using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class CoreSQLDbHelper
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        public CoreSQLDbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void BeginTransaction()
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _connection?.Close();
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _connection?.Close();
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                var dataTable = new DataTable();
                using (var adapter = new SqlDataAdapter(command))
                {
                    await connection.OpenAsync();
                    adapter.Fill(dataTable);
                }
                return dataTable;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, bool useTransaction = false)
        {
            if (useTransaction)
            {
                using (var command = new SqlCommand(query, _connection, _transaction))
                {
                    command.CommandType = CommandType.Text;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
