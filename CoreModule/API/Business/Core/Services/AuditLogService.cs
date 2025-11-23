using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BusinessLogic.Core.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly CoreSQLDbHelper _dbHelper;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(CoreSQLDbHelper dbHelper, ILogger<AuditLogService> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            var limit = Math.Clamp(filter.Limit, 1, 500);
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@limit", limit)
            };

            if (!string.IsNullOrWhiteSpace(filter.TableName))
            {
                conditions.Add("LOWER(al.table_name) = LOWER(@tableName)");
                parameters.Add(new NpgsqlParameter("@tableName", filter.TableName));
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                conditions.Add("UPPER(al.action) = UPPER(@action)");
                parameters.Add(new NpgsqlParameter("@action", filter.Action));
            }

            if (filter.StartDateUtc.HasValue)
            {
                conditions.Add("al.changed_at_utc >= @startDate");
                parameters.Add(new NpgsqlParameter("@startDate", filter.StartDateUtc.Value));
            }

            if (filter.EndDateUtc.HasValue)
            {
                conditions.Add("al.changed_at_utc <= @endDate");
                parameters.Add(new NpgsqlParameter("@endDate", filter.EndDateUtc.Value));
            }

            var whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                // Check if changed_by_user_id column exists
                const string checkColumnSql = @"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'core' 
                        AND table_name = 'audit_logs' 
                        AND column_name = 'changed_by_user_id'
                    );";

                using var checkCommand = new NpgsqlCommand(checkColumnSql, connection);
                var columnExists = (bool)(await checkCommand.ExecuteScalarAsync() ?? false);

                if (!columnExists)
                {
                    _logger.LogWarning("Column changed_by_user_id does not exist in audit_logs table. Please run the migration script.");
                    return Array.Empty<AuditLogDto>();
                }

            var sql = new StringBuilder();
            sql.AppendLine(@"
                SELECT 
                    al.audit_log_id,
                    al.table_name,
                    al.record_id,
                    al.action,
                    al.changed_by_user_id,
                    al.changed_at_utc,
                    al.ip_address,
                    al.user_agent,
                    u.full_name
                FROM core.audit_logs al
                LEFT JOIN core.users u ON u.user_id = al.changed_by_user_id");
            sql.AppendLine(whereClause);
            sql.AppendLine("ORDER BY al.changed_at_utc DESC");
            sql.AppendLine("LIMIT @limit;");

                using var command = new NpgsqlCommand(sql.ToString(), connection);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                using var reader = await command.ExecuteReaderAsync();
                var logs = new List<AuditLogDto>();

                while (await reader.ReadAsync())
                {
                    logs.Add(new AuditLogDto
                    {
                        AuditLogId = reader.GetInt64(0),
                        TableName = reader.GetString(1),
                        RecordId = reader.GetString(2),
                        Action = reader.GetString(3),
                        ChangedByUserId = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                        ChangedAtUtc = reader.GetDateTime(5),
                        IpAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
                        UserAgent = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ChangedByUserName = reader.IsDBNull(8) ? null : reader.GetString(8)
                    });
                }

                return logs;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01")
            {
                _logger.LogWarning("Attempted to query audit logs before the table existed.");
                return Array.Empty<AuditLogDto>();
            }
            catch (PostgresException ex) when (ex.SqlState == "42703")
            {
                _logger.LogWarning("Column changed_by_user_id does not exist in audit_logs table. Please run the migration script: migrations/sql/2025_11_23_fix_audit_logs_changed_by_user_id.sql");
                return Array.Empty<AuditLogDto>();
            }
        }
    }
}

