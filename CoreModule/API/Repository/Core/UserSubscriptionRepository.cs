using Entities.Core;
using Infrastructure;
using Microsoft.Data.SqlClient;
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
            var sql = "INSERT INTO core.UserSubscriptions (BusinessID, PlanID, Status, StartDateUTC, EndDateUTC, TrialEndsAtUTC, CreatedAtUTC, UpdatedAtUTC) OUTPUT INSERTED.SubscriptionID VALUES (@BusinessID, @PlanID, @Status, @StartDateUTC, @EndDateUTC, @TrialEndsAtUTC, GETUTCDATE(), GETUTCDATE())";
            var parameters = new[]
            {
                new SqlParameter("@BusinessID", userSubscription.BusinessID),
                new SqlParameter("@PlanID", userSubscription.PlanID),
                new SqlParameter("@Status", userSubscription.Status),
                new SqlParameter("@StartDateUTC", userSubscription.StartDateUTC),
                new SqlParameter("@EndDateUTC", userSubscription.EndDateUTC),
                new SqlParameter("@TrialEndsAtUTC", (object)userSubscription.TrialEndsAtUTC ?? DBNull.Value)
            };
            var id = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            userSubscription.SubscriptionID = (Guid)id;
            return userSubscription;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var sql = "DELETE FROM core.UserSubscriptions WHERE SubscriptionID = @SubscriptionID";
            var parameters = new[] { new SqlParameter("@SubscriptionID", id) };
            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<UserSubscription>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.UserSubscriptions";
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
            var sql = "SELECT * FROM core.UserSubscriptions WHERE SubscriptionID = @SubscriptionID";
            var parameters = new[] { new SqlParameter("@SubscriptionID", id) };
            var dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapToUserSubscription(dt.Rows[0]) : null;
        }

        public async Task<UserSubscription> UpdateAsync(UserSubscription userSubscription)
        {
            var sql = "UPDATE core.UserSubscriptions SET Status = @Status, EndDateUTC = @EndDateUTC, TrialEndsAtUTC = @TrialEndsAtUTC, UpdatedAtUTC = GETUTCDATE() WHERE SubscriptionID = @SubscriptionID";
            var parameters = new[]
            {
                new SqlParameter("@SubscriptionID", userSubscription.SubscriptionID),
                new SqlParameter("@Status", userSubscription.Status),
                new SqlParameter("@EndDateUTC", userSubscription.EndDateUTC),
                new SqlParameter("@TrialEndsAtUTC", (object)userSubscription.TrialEndsAtUTC ?? DBNull.Value)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return userSubscription;
        }

        private UserSubscription MapToUserSubscription(DataRow row)
        {
            return new UserSubscription
            {
                SubscriptionID = Guid.Parse(row["SubscriptionID"].ToString()),
                BusinessID = Guid.Parse(row["BusinessID"].ToString()),
                PlanID = Convert.ToInt32(row["PlanID"]),
                Status = row["Status"].ToString(),
                StartDateUTC = Convert.ToDateTime(row["StartDateUTC"]),
                EndDateUTC = Convert.ToDateTime(row["EndDateUTC"]),
                TrialEndsAtUTC = row["TrialEndsAtUTC"] != DBNull.Value ? Convert.ToDateTime(row["TrialEndsAtUTC"]) : (DateTime?)null,
                CreatedAtUTC = Convert.ToDateTime(row["CreatedAtUTC"]),
                UpdatedAtUTC = Convert.ToDateTime(row["UpdatedAtUTC"])
            };
        }
    }
}
