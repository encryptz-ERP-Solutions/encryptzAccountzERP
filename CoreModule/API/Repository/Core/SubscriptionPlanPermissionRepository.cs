using Entities.Core;
using Infrastructure;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repository.Core
{
    public class SubscriptionPlanPermissionRepository : ISubscriptionPlanPermissionRepository
    {
        private readonly CoreSQLDbHelper _dbHelper;

        public SubscriptionPlanPermissionRepository(CoreSQLDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<SubscriptionPlanPermission> CreateAsync(SubscriptionPlanPermission subscriptionPlanPermission)
        {
            var sql = "INSERT INTO core.subscription_plan_permissions (plan_id, permission_id) VALUES (@PlanID, @PermissionID)";
            var parameters = new[]
            {
                new NpgsqlParameter("@PlanID", subscriptionPlanPermission.PlanID),
                new NpgsqlParameter("@PermissionID", subscriptionPlanPermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlanPermission;
        }

        public async Task<bool> DeleteAsync(int planId, int permissionId)
        {
            var sql = "DELETE FROM core.subscription_plan_permissions WHERE plan_id = @PlanID AND permission_id = @PermissionID";
            var parameters = new[] 
            { 
                new NpgsqlParameter("@PlanID", planId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SubscriptionPlanPermission>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.subscription_plan_permissions";
            var dt = await _dbHelper.ExecuteQueryAsync(sql);
            var subscriptionPlanPermissions = new List<SubscriptionPlanPermission>();
            foreach (DataRow row in dt.Rows)
            {
                subscriptionPlanPermissions.Add(MapToSubscriptionPlanPermission(row));
            }
            return subscriptionPlanPermissions;
        }

        public async Task<SubscriptionPlanPermission> GetByIdAsync(int planId, int permissionId)
        {
            var sql = "SELECT * FROM core.subscription_plan_permissions WHERE plan_id = @PlanID AND permission_id = @PermissionID";
            var parameters = new[] 
            { 
                new NpgsqlParameter("@PlanID", planId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToSubscriptionPlanPermission(dt.Rows[0]) : null;
        }

        public async Task<IEnumerable<SubscriptionPlanPermission>> GetByPlanIdAsync(int planId)
        {
            var sql = "SELECT * FROM core.subscription_plan_permissions WHERE plan_id = @PlanID";
            var parameters = new[] { new NpgsqlParameter("@PlanID", planId) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            var subscriptionPlanPermissions = new List<SubscriptionPlanPermission>();
            foreach (DataRow row in dt.Rows)
            {
                subscriptionPlanPermissions.Add(MapToSubscriptionPlanPermission(row));
            }
            return subscriptionPlanPermissions;
        }

        public async Task<IEnumerable<SubscriptionPlanPermission>> GetByPermissionIdAsync(int permissionId)
        {
            var sql = "SELECT * FROM core.subscription_plan_permissions WHERE permission_id = @PermissionID";
            var parameters = new[] { new NpgsqlParameter("@PermissionID", permissionId) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            var subscriptionPlanPermissions = new List<SubscriptionPlanPermission>();
            foreach (DataRow row in dt.Rows)
            {
                subscriptionPlanPermissions.Add(MapToSubscriptionPlanPermission(row));
            }
            return subscriptionPlanPermissions;
        }

        public async Task<SubscriptionPlanPermission> UpdateAsync(SubscriptionPlanPermission subscriptionPlanPermission)
        {
            // For junction tables, typically we don't update the primary key
            // Instead, we delete the old record and create a new one
            // This method is included for completeness but may not be used in practice
            var sql = @"UPDATE core.subscription_plan_permissions 
                        SET plan_id = @PlanID, permission_id = @PermissionID 
                        WHERE plan_id = @OldPlanID AND permission_id = @OldPermissionID";
            var parameters = new[]
            {
                new NpgsqlParameter("@PlanID", subscriptionPlanPermission.PlanID),
                new NpgsqlParameter("@PermissionID", subscriptionPlanPermission.PermissionID),
                new NpgsqlParameter("@OldPlanID", subscriptionPlanPermission.PlanID),
                new NpgsqlParameter("@OldPermissionID", subscriptionPlanPermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlanPermission;
        }

        private SubscriptionPlanPermission MapToSubscriptionPlanPermission(DataRow row)
        {
            return new SubscriptionPlanPermission
            {
                PlanID = Convert.ToInt32(row["plan_id"]),
                PermissionID = Convert.ToInt32(row["permission_id"])
            };
        }
    }
}
