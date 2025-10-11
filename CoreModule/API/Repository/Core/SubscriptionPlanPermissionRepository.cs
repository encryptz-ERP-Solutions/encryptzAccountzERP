using Entities.Core;
using Infrastructure;
using Microsoft.Data.SqlClient;
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
            var sql = "INSERT INTO core.SubscriptionPlanPermissions (PlanID, PermissionID) VALUES (@PlanID, @PermissionID);";
            var parameters = new[]
            {
                new SqlParameter("@PlanID", subscriptionPlanPermission.PlanID),
                new SqlParameter("@PermissionID", subscriptionPlanPermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlanPermission;
        }

        public async Task<bool> DeleteAsync(int planId, int permissionId)
        {
            var sql = "DELETE FROM core.SubscriptionPlanPermissions WHERE PlanID = @PlanID AND PermissionID = @PermissionID";
            var parameters = new[] 
            { 
                new SqlParameter("@PlanID", planId),
                new SqlParameter("@PermissionID", permissionId)
            };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SubscriptionPlanPermission>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.SubscriptionPlanPermissions";
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
            var sql = "SELECT * FROM core.SubscriptionPlanPermissions WHERE PlanID = @PlanID AND PermissionID = @PermissionID";
            var parameters = new[] 
            { 
                new SqlParameter("@PlanID", planId),
                new SqlParameter("@PermissionID", permissionId)
            };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToSubscriptionPlanPermission(dt.Rows[0]) : null;
        }

        public async Task<IEnumerable<SubscriptionPlanPermission>> GetByPlanIdAsync(int planId)
        {
            var sql = "SELECT * FROM core.SubscriptionPlanPermissions WHERE PlanID = @PlanID";
            var parameters = new[] { new SqlParameter("@PlanID", planId) };
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
            var sql = "SELECT * FROM core.SubscriptionPlanPermissions WHERE PermissionID = @PermissionID";
            var parameters = new[] { new SqlParameter("@PermissionID", permissionId) };
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
            var sql = "UPDATE core.SubscriptionPlanPermissions SET PlanID = @PlanID, PermissionID = @PermissionID WHERE PlanID = @OldPlanID AND PermissionID = @OldPermissionID";
            var parameters = new[]
            {
                new SqlParameter("@PlanID", subscriptionPlanPermission.PlanID),
                new SqlParameter("@PermissionID", subscriptionPlanPermission.PermissionID),
                new SqlParameter("@OldPlanID", subscriptionPlanPermission.PlanID),
                new SqlParameter("@OldPermissionID", subscriptionPlanPermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlanPermission;
        }

        private SubscriptionPlanPermission MapToSubscriptionPlanPermission(DataRow row)
        {
            return new SubscriptionPlanPermission
            {
                PlanID = Convert.ToInt32(row["PlanID"]),
                PermissionID = Convert.ToInt32(row["PermissionID"])
            };
        }
    }
}
