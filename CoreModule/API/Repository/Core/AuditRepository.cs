using System;
using System.Data;
using System.Threading.Tasks;
using Infrastructure;
using Npgsql;
using Microsoft.Extensions.Logging;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<AuditRepository> _logger;

        public AuditRepository(IDbConnectionFactory connectionFactory, ILogger<AuditRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Logs an audit entry. Best-effort: if this fails, it logs the error but does not throw.
        /// </summary>
        public async Task LogAsync(Guid? userId, string action, string objectType, string objectId, string notes = null)
        {
            try
            {
                var query = @"
                    INSERT INTO core.audit_logs 
                        (table_name, record_id, action, changed_by_user_id, changed_at_utc, new_values)
                    VALUES 
                        (@table_name, @record_id, @action, @changed_by_user_id, NOW() AT TIME ZONE 'UTC', @new_values::jsonb);";

                var connection = _connectionFactory.CreateConnection();
                var npgsqlConnection = (NpgsqlConnection)connection;
                await npgsqlConnection.OpenAsync();
                
                using var command = new NpgsqlCommand(query, npgsqlConnection);
                command.Parameters.AddWithValue("@table_name", objectType);
                command.Parameters.AddWithValue("@record_id", objectId);
                command.Parameters.AddWithValue("@action", action);
                command.Parameters.AddWithValue("@changed_by_user_id", userId.HasValue ? (object)userId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@new_values", notes ?? "{}");

                await command.ExecuteNonQueryAsync();
                await npgsqlConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                // Best-effort: log the failure but don't throw
                _logger.LogError(ex, "Failed to write audit log entry for {Action} on {ObjectType}/{ObjectId}", 
                    action, objectType, objectId);
            }
        }
    }
}

