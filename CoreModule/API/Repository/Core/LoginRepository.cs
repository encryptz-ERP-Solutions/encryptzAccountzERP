using System;
using System.Data;
using System.Threading.Tasks;
using Data.Core;
using Microsoft.Data.SqlClient;
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
            var query = "UPDATE core.Users SET HashedPassword = @HashedPassword, UpdatedAtUTC = @UpdatedAtUTC WHERE UserID = @UserID";
            var parameters = new[] {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@HashedPassword", newHashedPassword),
                new SqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            int result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> SaveOTPAsync(string loginIdentifier, string otp)
        {
            var query = @"
                DELETE FROM core.OneTimePasswords WHERE LoginIdentifier = @LoginIdentifier AND IsUsed = 0;

                INSERT INTO core.OneTimePasswords (LoginIdentifier, OTP, ExpiryTimeUTC, IsUsed, CreatedAtUTC)
                VALUES (@LoginIdentifier, @OTP, @ExpiryTimeUTC, 0, @CreatedAtUTC)";

            var parameters = new[] {
                new SqlParameter("@LoginIdentifier", loginIdentifier),
                new SqlParameter("@OTP", otp),
                new SqlParameter("@ExpiryTimeUTC", DateTime.UtcNow.AddMinutes(10)), // OTP valid for 10 minutes
                new SqlParameter("@CreatedAtUTC", DateTime.UtcNow)
            };

            int rows = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rows > 0;
        }

        public async Task<bool> VerifyOTPAsync(string loginIdentifier, string otp)
        {
            var query = "SELECT OtpID FROM core.OneTimePasswords WHERE LoginIdentifier = @LoginIdentifier AND OTP = @OTP AND ExpiryTimeUTC > GETUTCDATE() AND IsUsed = 0";
            var parameters = new[] {
                new SqlParameter("@LoginIdentifier", loginIdentifier),
                new SqlParameter("@OTP", otp),
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                return false; // OTP not found, expired, or already used
            }

            // Mark OTP as used to prevent reuse
            var otpId = Convert.ToInt64(dataTable.Rows[0]["OtpID"]);
            var updateQuery = "UPDATE core.OneTimePasswords SET IsUsed = 1 WHERE OtpID = @OtpID";
            var updateParameters = new[] { new SqlParameter("@OtpID", otpId) };

            await _sqlHelper.ExecuteNonQueryAsync(updateQuery, updateParameters);

            return true;
        }
    }
}