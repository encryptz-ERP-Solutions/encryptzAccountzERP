using Entities.Core;
using Infrastructure;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repository.Core
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly CoreSQLDbHelper _dbHelper;

        public SubscriptionPlanRepository(CoreSQLDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<SubscriptionPlan> CreateAsync(SubscriptionPlan subscriptionPlan)
        {
            var sql = "INSERT INTO core.SubscriptionPlans (PlanName, Description, Price, MaxUsers, MaxBusinesses, IsPubliclyVisible, IsActive) VALUES (@PlanName, @Description, @Price, @MaxUsers, @MaxBusinesses, @IsPubliclyVisible, @IsActive); SELECT SCOPE_IDENTITY();";
            var parameters = new[]
            {
                new SqlParameter("@PlanName", subscriptionPlan.PlanName),
                new SqlParameter("@Description", subscriptionPlan.Description),
                new SqlParameter("@Price", subscriptionPlan.Price),
                new SqlParameter("@MaxUsers", subscriptionPlan.MaxUsers),
                new SqlParameter("@MaxBusinesses", subscriptionPlan.MaxBusinesses),
                new SqlParameter("@IsPubliclyVisible", subscriptionPlan.IsPubliclyVisible),
                new SqlParameter("@IsActive", subscriptionPlan.IsActive)
            };
            var id = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            subscriptionPlan.PlanID = Convert.ToInt32(id);
            return subscriptionPlan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM core.SubscriptionPlans WHERE PlanID = @PlanID";
            var parameters = new[] { new SqlParameter("@PlanID", id) };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.SubscriptionPlans";
            var dt = await _dbHelper.ExecuteQueryAsync(sql);
            var subscriptionPlans = new List<SubscriptionPlan>();
            foreach (DataRow row in dt.Rows)
            {
                subscriptionPlans.Add(MapToSubscriptionPlan(row));
            }
            return subscriptionPlans;
        }

        public async Task<SubscriptionPlan> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM core.SubscriptionPlans WHERE PlanID = @PlanID";
            var parameters = new[] { new SqlParameter("@PlanID", id) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToSubscriptionPlan(dt.Rows[0]) : null;
        }

        public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan subscriptionPlan)
        {
            var sql = "UPDATE core.SubscriptionPlans SET PlanName = @PlanName, Description = @Description, Price = @Price, MaxUsers = @MaxUsers, MaxBusinesses = @MaxBusinesses, IsPubliclyVisible = @IsPubliclyVisible, IsActive = @IsActive WHERE PlanID = @PlanID";
            var parameters = new[]
            {
                new SqlParameter("@PlanID", subscriptionPlan.PlanID),
                new SqlParameter("@PlanName", subscriptionPlan.PlanName),
                new SqlParameter("@Description", subscriptionPlan.Description),
                new SqlParameter("@Price", subscriptionPlan.Price),
                new SqlParameter("@MaxUsers", subscriptionPlan.MaxUsers),
                new SqlParameter("@MaxBusinesses", subscriptionPlan.MaxBusinesses),
                new SqlParameter("@IsPubliclyVisible", subscriptionPlan.IsPubliclyVisible),
                new SqlParameter("@IsActive", subscriptionPlan.IsActive)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlan;
        }

        private SubscriptionPlan MapToSubscriptionPlan(DataRow row)
        {
            return new SubscriptionPlan
            {
                PlanID = Convert.ToInt32(row["PlanID"]),
                PlanName = row["PlanName"].ToString(),
                Description = row["Description"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                MaxUsers = Convert.ToInt32(row["MaxUsers"]),
                MaxBusinesses = Convert.ToInt32(row["MaxBusinesses"]),
                IsPubliclyVisible = Convert.ToBoolean(row["IsPubliclyVisible"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
