using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BusinessLogic.Admin.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly CoreSQLDbHelper _dbHelper;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(CoreSQLDbHelper dbHelper, ILogger<AdminDashboardService> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public async Task<AdminDashboardSummaryDto> GetSummaryAsync(int recentLimit = 5)
        {
            var limit = Math.Clamp(recentLimit, 1, 15);

            var summary = new AdminDashboardSummaryDto();

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                summary.Kpis = await GetKpisAsync(connection);
                summary.RecentUsers = await GetRecentUsersAsync(connection, limit);
                summary.RecentBusinesses = await GetRecentBusinessesAsync(connection, limit);
                summary.PlanUsage = await GetPlanUsageAsync(connection);
                summary.RecentActivity = await GetRecentActivityAsync(connection, limit);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build admin dashboard summary");
                throw;
            }
        }

        private static async Task<AdminDashboardKpiDto> GetKpisAsync(NpgsqlConnection connection)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM core.users) AS total_users,
                    (SELECT COUNT(*) FROM core.users WHERE is_active = TRUE) AS active_users,
                    (SELECT COUNT(*) FROM core.businesses) AS total_businesses,
                    (SELECT COUNT(*) FROM core.businesses WHERE is_active = TRUE) AS active_businesses,
                    (SELECT COUNT(*) FROM core.roles) AS total_roles,
                    (SELECT COUNT(*) FROM core.permissions) AS total_permissions,
                    (SELECT COUNT(*) FROM core.modules) AS total_modules,
                    (SELECT COUNT(*) FROM core.menu_items) AS total_menu_items,
                    (SELECT COUNT(*) FROM core.subscription_plans) AS subscription_plans;
            ";

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            var kpis = new AdminDashboardKpiDto();
            if (await reader.ReadAsync())
            {
                kpis.TotalUsers = reader.GetInt32(0);
                kpis.ActiveUsers = reader.GetInt32(1);
                kpis.TotalBusinesses = reader.GetInt32(2);
                kpis.ActiveBusinesses = reader.GetInt32(3);
                kpis.TotalRoles = reader.GetInt32(4);
                kpis.TotalPermissions = reader.GetInt32(5);
                kpis.TotalModules = reader.GetInt32(6);
                kpis.TotalMenuItems = reader.GetInt32(7);
                kpis.SubscriptionPlans = reader.GetInt32(8);
            }

            await reader.CloseAsync();
            return kpis;
        }

        private static async Task<List<AdminUserSummaryDto>> GetRecentUsersAsync(NpgsqlConnection connection, int limit)
        {
            const string sql = @"
                SELECT user_id, user_handle, full_name, email, is_active, created_at_utc
                FROM core.users
                ORDER BY created_at_utc DESC
                LIMIT @limit;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            var users = new List<AdminUserSummaryDto>();

            while (await reader.ReadAsync())
            {
                users.Add(new AdminUserSummaryDto
                {
                    UserId = reader.GetGuid(0),
                    UserHandle = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAtUtc = reader.GetDateTime(5)
                });
            }

            await reader.CloseAsync();
            return users;
        }

        private static async Task<List<AdminBusinessSummaryDto>> GetRecentBusinessesAsync(NpgsqlConnection connection, int limit)
        {
            const string sql = @"
                SELECT business_id, business_name, city, state_id, is_active, created_at_utc
                FROM core.businesses
                ORDER BY created_at_utc DESC
                LIMIT @limit;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            var businesses = new List<AdminBusinessSummaryDto>();

            while (await reader.ReadAsync())
            {
                businesses.Add(new AdminBusinessSummaryDto
                {
                    BusinessId = reader.GetGuid(0),
                    BusinessName = reader.GetString(1),
                    City = reader.IsDBNull(2) ? null : reader.GetString(2),
                    State = null, // state_id is an integer FK, not a state name string - set to null for now
                    IsActive = reader.GetBoolean(4),
                    CreatedAtUtc = reader.GetDateTime(5)
                });
            }

            await reader.CloseAsync();
            return businesses;
        }

        private static async Task<List<AdminPlanUsageDto>> GetPlanUsageAsync(NpgsqlConnection connection)
        {
            const string sql = @"
                WITH active_subscriptions AS (
                    SELECT plan_id, COUNT(*) AS subscription_count
                    FROM core.user_subscriptions
                    WHERE status::text IN ('Active', 'Trial', 'active', 'trial')
                    GROUP BY plan_id
                )
                SELECT 
                    sp.plan_id,
                    sp.plan_name,
                    sp.price,
                    sp.is_active,
                    COALESCE(active_subscriptions.subscription_count, 0) AS active_subscriptions
                FROM core.subscription_plans sp
                LEFT JOIN active_subscriptions ON active_subscriptions.plan_id = sp.plan_id
                ORDER BY sp.plan_name;
            ";

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            var plans = new List<AdminPlanUsageDto>();

            while (await reader.ReadAsync())
            {
                plans.Add(new AdminPlanUsageDto
                {
                    PlanId = reader.GetInt32(0),
                    PlanName = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    IsActive = reader.GetBoolean(3),
                    ActiveSubscriptions = reader.GetInt32(4)
                });
            }

            await reader.CloseAsync();
            return plans;
        }

        private async Task<List<AdminActivityDto>> GetRecentActivityAsync(NpgsqlConnection connection, int limit)
        {
            // Check if audit_logs table exists and has the changed_by_user_id column
            const string checkTableSql = @"
                SELECT EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_schema = 'core' 
                    AND table_name = 'audit_logs' 
                    AND column_name = 'changed_by_user_id'
                );";

            try
            {
                using var checkCommand = new NpgsqlCommand(checkTableSql, connection);
                var tableExists = (bool)(await checkCommand.ExecuteScalarAsync() ?? false);

                if (!tableExists)
                {
                    _logger.LogWarning("Audit log table or changed_by_user_id column not found. Returning empty activity list.");
                    return new List<AdminActivityDto>();
                }

                const string sql = @"
                    SELECT 
                        al.audit_log_id,
                        al.table_name,
                        al.action,
                        al.record_id,
                        al.changed_at_utc,
                        u.full_name
                    FROM core.audit_logs al
                    LEFT JOIN core.users u ON u.user_id = al.changed_by_user_id
                    ORDER BY al.changed_at_utc DESC
                    LIMIT @limit;";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@limit", limit);

                using var reader = await command.ExecuteReaderAsync();
                var activities = new List<AdminActivityDto>();

                while (await reader.ReadAsync())
                {
                    activities.Add(new AdminActivityDto
                    {
                        AuditLogId = reader.GetInt64(0),
                        TableName = reader.GetString(1),
                        Action = reader.GetString(2),
                        RecordId = reader.GetString(3),
                        ChangedAtUtc = reader.GetDateTime(4),
                        ChangedByUserName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Description = BuildActivityDescription(reader.GetString(1), reader.GetString(2))
                    });
                }

                await reader.CloseAsync();
                return activities;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01" || ex.SqlState == "42703")
            {
                // audit_logs table not found or column doesn't exist
                _logger.LogWarning("Audit log table or column not found while building admin dashboard summary. Returning empty activity list. Error: {Error}", ex.Message);
                return new List<AdminActivityDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity from audit logs");
                return new List<AdminActivityDto>();
            }
        }

        private static string BuildActivityDescription(string tableName, string action)
        {
            var actionText = action switch
            {
                "INSERT" => "created",
                "UPDATE" => "updated",
                "DELETE" => "deleted",
                _ => action.ToLowerInvariant()
            };

            return $"{tableName} {actionText}";
        }
    }
}

