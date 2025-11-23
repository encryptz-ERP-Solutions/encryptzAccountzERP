using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    /// <summary>
    /// Service implementation for dashboard operations
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly CoreSQLDbHelper _dbHelper;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(CoreSQLDbHelper dbHelper, ILogger<DashboardService> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        /// <summary>
        /// Get complete dashboard data for a business
        /// </summary>
        public async Task<DashboardResponseDto> GetDashboardDataAsync(Guid businessId, DashboardRequestDto request)
        {
            // Validate pagination parameters
            var limit = Math.Min(Math.Max(request.Limit, 1), 100); // Between 1 and 100
            var offset = Math.Max(request.Offset, 0);

            // Fetch all data in parallel for better performance
            var kpisTask = GetKpisAsync(businessId);
            var activitiesTask = GetRecentActivitiesAsync(businessId, limit, offset);
            var subscriptionTask = GetSubscriptionStatusAsync(businessId);

            await Task.WhenAll(kpisTask, activitiesTask, subscriptionTask);

            return new DashboardResponseDto
            {
                Kpis = await kpisTask,
                RecentActivities = await activitiesTask,
                Shortcuts = GetShortcuts(),
                SubscriptionStatus = await subscriptionTask
            };
        }

        /// <summary>
        /// Calculate KPIs from transaction data
        /// </summary>
        public async Task<DashboardKpisDto> GetKpisAsync(Guid businessId)
        {
            const string sql = @"
                WITH account_balances AS (
                    SELECT 
                        coa.account_id,
                        coa.account_type_id,
                        at.account_type_name,
                        SUM(COALESCE(td.debit_amount, 0) - COALESCE(td.credit_amount, 0)) AS balance
                    FROM acct.chart_of_accounts coa
                    LEFT JOIN acct.transaction_details td ON td.account_id = coa.account_id
                    LEFT JOIN acct.transaction_headers th ON th.transaction_header_id = td.transaction_header_id
                    LEFT JOIN acct.account_types at ON at.account_type_id = coa.account_type_id
                    WHERE coa.business_id = @businessId
                        AND coa.is_active = TRUE
                    GROUP BY coa.account_id, coa.account_type_id, at.account_type_name
                )
                SELECT 
                    -- Receivables: Debit balance in Asset accounts that are receivables
                    COALESCE(SUM(CASE 
                        WHEN account_type_name = 'Asset' AND balance > 0 
                        THEN balance 
                        ELSE 0 
                    END), 0) AS receivables,
                    
                    -- Payables: Credit balance in Liability accounts
                    COALESCE(ABS(SUM(CASE 
                        WHEN account_type_name = 'Liability' AND balance < 0 
                        THEN balance 
                        ELSE 0 
                    END)), 0) AS payables,
                    
                    -- Cash: Balance in Asset accounts (typically cash accounts)
                    COALESCE(SUM(CASE 
                        WHEN account_type_name = 'Asset' 
                        THEN balance 
                        ELSE 0 
                    END), 0) AS cash,
                    
                    -- Revenue: Credit balance in Revenue accounts
                    COALESCE(ABS(SUM(CASE 
                        WHEN account_type_name = 'Revenue' 
                        THEN balance 
                        ELSE 0 
                    END)), 0) AS revenue,
                    
                    -- Expenses: Debit balance in Expense accounts
                    COALESCE(SUM(CASE 
                        WHEN account_type_name = 'Expense' 
                        THEN balance 
                        ELSE 0 
                    END), 0) AS expenses
                FROM account_balances;
            ";

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@businessId", businessId);

                using var reader = await command.ExecuteReaderAsync();
                
                var kpis = new DashboardKpisDto();
                
                if (await reader.ReadAsync())
                {
                    kpis.Receivables = reader.GetDecimal(0);
                    kpis.Payables = reader.GetDecimal(1);
                    kpis.Cash = reader.GetDecimal(2);
                    kpis.Revenue = reader.GetDecimal(3);
                    kpis.Expenses = reader.GetDecimal(4);
                    kpis.NetProfit = kpis.Revenue - kpis.Expenses;
                }

                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching KPIs for business {BusinessId}", businessId);
                throw;
            }
        }

        /// <summary>
        /// Get recent activities from audit logs
        /// </summary>
        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(Guid businessId, int limit, int offset)
        {
            const string sql = @"
                SELECT 
                    al.audit_log_id,
                    al.table_name,
                    al.record_id,
                    al.action,
                    u.full_name AS changed_by_user_name,
                    al.changed_at_utc
                FROM core.audit_logs al
                LEFT JOIN core.users u ON u.user_id = al.changed_by_user_id
                WHERE al.table_name IN (
                    'businesses', 
                    'chart_of_accounts', 
                    'transaction_headers', 
                    'transaction_details',
                    'user_businesses',
                    'user_business_roles'
                )
                AND (
                    -- For business-related tables, match the business_id in new_values
                    al.new_values->>'business_id' = @businessId::text
                    OR al.old_values->>'business_id' = @businessId::text
                    OR al.record_id = @businessId::text
                )
                ORDER BY al.changed_at_utc DESC
                LIMIT @limit
                OFFSET @offset;
            ";

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@businessId", businessId);
                command.Parameters.AddWithValue("@limit", limit);
                command.Parameters.AddWithValue("@offset", offset);

                using var reader = await command.ExecuteReaderAsync();
                
                var activities = new List<RecentActivityDto>();
                
                while (await reader.ReadAsync())
                {
                    var activity = new RecentActivityDto
                    {
                        AuditLogId = reader.GetInt64(0),
                        TableName = reader.GetString(1),
                        RecordId = reader.GetString(2),
                        Action = reader.GetString(3),
                        ChangedByUserName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ChangedAtUtc = reader.GetDateTime(5),
                        Description = GenerateActivityDescription(reader.GetString(3), reader.GetString(1))
                    };
                    
                    activities.Add(activity);
                }

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent activities for business {BusinessId}", businessId);
                throw;
            }
        }

        /// <summary>
        /// Get subscription status for a business
        /// </summary>
        public async Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(Guid businessId)
        {
            const string sql = @"
                SELECT 
                    sp.plan_name,
                    us.status,
                    us.start_date_utc,
                    us.end_date_utc,
                    us.trial_ends_at_utc,
                    EXTRACT(DAY FROM (us.end_date_utc - NOW())) AS days_remaining
                FROM core.user_subscriptions us
                INNER JOIN core.subscription_plans sp ON sp.plan_id = us.plan_id
                WHERE us.business_id = @businessId
                ORDER BY us.created_at_utc DESC
                LIMIT 1;
            ";

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@businessId", businessId);

                using var reader = await command.ExecuteReaderAsync();
                
                var status = new SubscriptionStatusDto
                {
                    PlanName = "Free",
                    Status = "None",
                    DaysRemaining = 0,
                    IsTrialActive = false
                };
                
                if (await reader.ReadAsync())
                {
                    status.PlanName = reader.GetString(0);
                    status.Status = reader.GetString(1);
                    status.StartDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                    status.EndDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3);
                    status.TrialEndsAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4);
                    status.DaysRemaining = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetDouble(5));
                    status.IsTrialActive = status.TrialEndsAt.HasValue && status.TrialEndsAt.Value > DateTime.UtcNow;
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscription status for business {BusinessId}", businessId);
                throw;
            }
        }

        /// <summary>
        /// Get predefined shortcuts for the dashboard
        /// </summary>
        private List<DashboardShortcutDto> GetShortcuts()
        {
            return new List<DashboardShortcutDto>
            {
                new DashboardShortcutDto
                {
                    Label = "New Transaction",
                    Icon = "add_circle",
                    Route = "/transactions/new",
                    Description = "Create a new accounting transaction"
                },
                new DashboardShortcutDto
                {
                    Label = "Chart of Accounts",
                    Icon = "account_tree",
                    Route = "/accounts/chart",
                    Description = "View and manage chart of accounts"
                },
                new DashboardShortcutDto
                {
                    Label = "Reports",
                    Icon = "assessment",
                    Route = "/reports",
                    Description = "View financial reports"
                },
                new DashboardShortcutDto
                {
                    Label = "Settings",
                    Icon = "settings",
                    Route = "/settings",
                    Description = "Business settings and preferences"
                }
            };
        }

        /// <summary>
        /// Generate human-readable description for activity
        /// </summary>
        private string GenerateActivityDescription(string action, string tableName)
        {
            var friendlyTableName = tableName.Replace("_", " ").ToLower();
            
            return action.ToUpper() switch
            {
                "INSERT" => $"Created new {friendlyTableName}",
                "UPDATE" => $"Updated {friendlyTableName}",
                "DELETE" => $"Deleted {friendlyTableName}",
                _ => $"{action} on {friendlyTableName}"
            };
        }
    }
}
