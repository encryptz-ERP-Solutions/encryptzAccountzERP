using Entities.Core;
using Infrastructure;
using Npgsql;
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
            var sql = @"INSERT INTO core.subscription_plans (plan_name, description, price, max_users, max_businesses, is_publicly_visible, is_active) 
                        VALUES (@PlanName, @Description, @Price, @MaxUsers, @MaxBusinesses, @IsPubliclyVisible, @IsActive)
                        RETURNING plan_id";
            var parameters = new[]
            {
                new NpgsqlParameter("@PlanName", subscriptionPlan.PlanName),
                new NpgsqlParameter("@Description", subscriptionPlan.Description),
                new NpgsqlParameter("@Price", subscriptionPlan.Price),
                new NpgsqlParameter("@MaxUsers", subscriptionPlan.MaxUsers),
                new NpgsqlParameter("@MaxBusinesses", subscriptionPlan.MaxBusinesses),
                new NpgsqlParameter("@IsPubliclyVisible", subscriptionPlan.IsPubliclyVisible),
                new NpgsqlParameter("@IsActive", subscriptionPlan.IsActive)
            };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                subscriptionPlan.PlanID = Convert.ToInt32(dt.Rows[0]["plan_id"]);
            }
            return subscriptionPlan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM core.subscription_plans WHERE plan_id = @PlanID";
            var parameters = new[] { new NpgsqlParameter("@PlanID", id) };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.subscription_plans ORDER BY plan_name";
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
            var sql = "SELECT * FROM core.subscription_plans WHERE plan_id = @PlanID";
            var parameters = new[] { new NpgsqlParameter("@PlanID", id) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToSubscriptionPlan(dt.Rows[0]) : null;
        }

        public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan subscriptionPlan)
        {
            var sql = @"UPDATE core.subscription_plans 
                        SET plan_name = @PlanName, description = @Description, price = @Price, 
                            max_users = @MaxUsers, max_businesses = @MaxBusinesses, 
                            is_publicly_visible = @IsPubliclyVisible, is_active = @IsActive 
                        WHERE plan_id = @PlanID";
            var parameters = new[]
            {
                new NpgsqlParameter("@PlanID", subscriptionPlan.PlanID),
                new NpgsqlParameter("@PlanName", subscriptionPlan.PlanName),
                new NpgsqlParameter("@Description", subscriptionPlan.Description),
                new NpgsqlParameter("@Price", subscriptionPlan.Price),
                new NpgsqlParameter("@MaxUsers", subscriptionPlan.MaxUsers),
                new NpgsqlParameter("@MaxBusinesses", subscriptionPlan.MaxBusinesses),
                new NpgsqlParameter("@IsPubliclyVisible", subscriptionPlan.IsPubliclyVisible),
                new NpgsqlParameter("@IsActive", subscriptionPlan.IsActive)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return subscriptionPlan;
        }

        private SubscriptionPlan MapToSubscriptionPlan(DataRow row)
        {
            return new SubscriptionPlan
            {
                PlanID = Convert.ToInt32(row["plan_id"]),
                PlanName = row["plan_name"].ToString(),
                Description = row["description"] == DBNull.Value ? null : row["description"].ToString(),
                Price = Convert.ToDecimal(row["price"]),
                MaxUsers = Convert.ToInt32(row["max_users"]),
                MaxBusinesses = Convert.ToInt32(row["max_businesses"]),
                IsPubliclyVisible = Convert.ToBoolean(row["is_publicly_visible"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            };
        }
    }
}
