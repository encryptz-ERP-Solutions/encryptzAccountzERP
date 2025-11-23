using Entities.Core;
using Infrastructure;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repository.Core
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly CoreSQLDbHelper _dbHelper;

        public UserSubscriptionRepository(CoreSQLDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<UserSubscription> CreateAsync(UserSubscription userSubscription)
        {
            var sql = @"INSERT INTO core.user_subscriptions (business_id, plan_id, status, start_date_utc, end_date_utc, trial_ends_at_utc, created_at_utc, updated_at_utc) 
                        VALUES (@BusinessID, @PlanID, @Status, @StartDateUTC, @EndDateUTC, @TrialEndsAtUTC, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC')
                        RETURNING subscription_id";
            var parameters = new[]
            {
                new NpgsqlParameter("@BusinessID", userSubscription.BusinessID),
                new NpgsqlParameter("@PlanID", userSubscription.PlanID),
                new NpgsqlParameter("@Status", userSubscription.Status),
                new NpgsqlParameter("@StartDateUTC", userSubscription.StartDateUTC),
                new NpgsqlParameter("@EndDateUTC", userSubscription.EndDateUTC),
                new NpgsqlParameter("@TrialEndsAtUTC", (object)userSubscription.TrialEndsAtUTC ?? DBNull.Value)
            };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                userSubscription.SubscriptionID = (Guid)dt.Rows[0]["subscription_id"];
            }
            return userSubscription;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var sql = "DELETE FROM core.user_subscriptions WHERE subscription_id = @SubscriptionID";
            var parameters = new[] { new NpgsqlParameter("@SubscriptionID", id) };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<UserSubscription>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.user_subscriptions ORDER BY created_at_utc DESC";
            var dt = await _dbHelper.ExecuteQueryAsync(sql);
            var userSubscriptions = new List<UserSubscription>();
            foreach (DataRow row in dt.Rows)
            {
                userSubscriptions.Add(MapToUserSubscription(row));
            }
            return userSubscriptions;
        }

        public async Task<UserSubscription> GetByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM core.user_subscriptions WHERE subscription_id = @SubscriptionID";
            var parameters = new[] { new NpgsqlParameter("@SubscriptionID", id) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToUserSubscription(dt.Rows[0]) : null;
        }

        public async Task<UserSubscription> UpdateAsync(UserSubscription userSubscription)
        {
            var sql = @"UPDATE core.user_subscriptions 
                        SET status = @Status, end_date_utc = @EndDateUTC, trial_ends_at_utc = @TrialEndsAtUTC, 
                            updated_at_utc = NOW() AT TIME ZONE 'UTC' 
                        WHERE subscription_id = @SubscriptionID";
            var parameters = new[]
            {
                new NpgsqlParameter("@SubscriptionID", userSubscription.SubscriptionID),
                new NpgsqlParameter("@Status", userSubscription.Status),
                new NpgsqlParameter("@EndDateUTC", userSubscription.EndDateUTC),
                new NpgsqlParameter("@TrialEndsAtUTC", (object)userSubscription.TrialEndsAtUTC ?? DBNull.Value)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return userSubscription;
        }

        private UserSubscription MapToUserSubscription(DataRow row)
        {
            return new UserSubscription
            {
                SubscriptionID = (Guid)row["subscription_id"],
                BusinessID = (Guid)row["business_id"],
                PlanID = Convert.ToInt32(row["plan_id"]),
                Status = row["status"].ToString(),
                StartDateUTC = Convert.ToDateTime(row["start_date_utc"]),
                EndDateUTC = Convert.ToDateTime(row["end_date_utc"]),
                TrialEndsAtUTC = row["trial_ends_at_utc"] != DBNull.Value ? Convert.ToDateTime(row["trial_ends_at_utc"]) : (DateTime?)null,
                CreatedAtUTC = Convert.ToDateTime(row["created_at_utc"]),
                UpdatedAtUTC = Convert.ToDateTime(row["updated_at_utc"])
            };
        }
    }
}
