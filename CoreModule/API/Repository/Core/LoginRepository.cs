using System;
using System.Data;
using System.Threading.Tasks;
using Infrastructure;
using Npgsql;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class LoginRepository : ILoginRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public LoginRepository(CoreSQLDbHelper coreSQLDbHelper)
        {
            _sqlHelper = coreSQLDbHelper;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string newHashedPassword)
        {
            var query = "UPDATE core.users SET hashed_password = @HashedPassword, updated_at_utc = @UpdatedAtUTC WHERE user_id = @UserID";
            var parameters = new[] {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@HashedPassword", newHashedPassword),
                new NpgsqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            int result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> SaveOTPAsync(string loginIdentifier, string otp)
        {
            var query = @"
                DELETE FROM core.one_time_passwords WHERE login_identifier = @LoginIdentifier AND is_used = FALSE;

                INSERT INTO core.one_time_passwords (login_identifier, otp, expiry_time_utc, is_used, created_at_utc)
                VALUES (@LoginIdentifier, @OTP, @ExpiryTimeUTC, FALSE, @CreatedAtUTC)";

            var parameters = new[] {
                new NpgsqlParameter("@LoginIdentifier", loginIdentifier),
                new NpgsqlParameter("@OTP", otp),
                new NpgsqlParameter("@ExpiryTimeUTC", DateTime.UtcNow.AddMinutes(10)), // OTP valid for 10 minutes
                new NpgsqlParameter("@CreatedAtUTC", DateTime.UtcNow)
            };

            int rows = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rows > 0;
        }

        public async Task<bool> VerifyOTPAsync(string loginIdentifier, string otp)
        {
            var query = "SELECT otp_id FROM core.one_time_passwords WHERE login_identifier = @LoginIdentifier AND otp = @OTP AND expiry_time_utc > NOW() AT TIME ZONE 'UTC' AND is_used = FALSE";
            var parameters = new[] {
                new NpgsqlParameter("@LoginIdentifier", loginIdentifier),
                new NpgsqlParameter("@OTP", otp),
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                return false; // OTP not found, expired, or already used
            }

            // Mark OTP as used to prevent reuse
            var otpId = Convert.ToInt64(dataTable.Rows[0]["otp_id"]);
            var updateQuery = "UPDATE core.one_time_passwords SET is_used = TRUE WHERE otp_id = @OtpID";
            var updateParameters = new[] { new NpgsqlParameter("@OtpID", otpId) };

            await _sqlHelper.ExecuteNonQueryAsync(updateQuery, updateParameters);

            return true;
        }
    }
}