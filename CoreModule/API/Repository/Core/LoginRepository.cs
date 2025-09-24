using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Data.Core;
using Entities.Admin;
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
        public Task<User> LoginAsync(string userId, string password)
        {
            try
            {
                User user = new User();
                var query = "SELECT * FROM core.userM WHERE userId = @userId and userPassword= @userPassword";
                var parameters = new[] {
                    new SqlParameter("@userId",userId),
                    new SqlParameter("@userPassword",password)
                };
                var dataTable = _sqlHelper.ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count == 0) return Task.FromResult(user);

                user = MapDataRowToUser(dataTable.Rows[0]);
                return Task.FromResult(user);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ChangePassword(int userId, string newpassword)
        {
            try
            {               
                var query = "update core.userM set userPassword= @userPassword WHERE userId = @userId";
                var parameters = new[] {
                    new SqlParameter("@userId",userId),
                    new SqlParameter("@userPassword",newpassword)
                };
                int result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);

                return result > 0;

            }
            catch (Exception)
            {
                throw;
            }
        }

        // Generate and store OTP in database
        public async Task<bool> SaveOTP(string loginType, string loginId, string otp, string fullName)
        {
            try
            {
                string query = "INSERT INTO core.loginOTP (LoginType, LoginId, FullName, OTP, ExpiryTime, IsUsed, EntryTime) VALUES (@LoginType, @LoginId, @FullName, @OTP, @ExpiryTime, 0, @EntryTime)";
                var parameters = new[] {
                    new SqlParameter("@LoginType", loginType),
                    new SqlParameter("@LoginId", loginId),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@OTP", otp),
                    new SqlParameter("@ExpiryTime", DateTime.UtcNow.AddMinutes(5)),
                    new SqlParameter("@EntryTime",DateTime.UtcNow)
                 };

                int rows = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
                return rows > 0;
            }
            catch (Exception)
            {

                throw;
            }            
        }

        // Get user by email
        public async Task<int?> GetUserIdByEmail(string email)
        {
            try
            {
                string query = "SELECT Id FROM core.userM WHERE Email = @Email";
                var parameters = new[] {
                    new SqlParameter("@Email", email)
                };
                DataTable result = _sqlHelper.ExecuteQuery(query, parameters);
                return result == null ? (int?)null : Convert.ToInt32(result.Rows[0][0]);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Verify OTP
        public async Task<bool> VerifyOTP(string loginType, string LoginId, string otp)
        {
            try
            {
                string query = "SELECT Id FROM core.loginOTP WHERE LoginId = @LoginId AND LoginType = @LoginType AND OTP = @OTP AND ExpiryTime > GETUTCDATE() AND IsUsed = 0";
                var parameters = new[] {
                    new SqlParameter("@LoginType", loginType),
                        new SqlParameter("@LoginId", LoginId),
                        new SqlParameter("@OTP", otp),
                };

                DataTable result = _sqlHelper.ExecuteQuery(query, parameters);

                if (result != null)
                {
                    if (result.Rows.Count == 0) return false; 
                    // Mark OTP as used
                    string updateQuery = "UPDATE core.loginOTP SET IsUsed = 1 WHERE Id = @Id";

                    parameters = new[] {
                        new SqlParameter("@Id", result.Rows[0]["Id"])
                    };
                    await _sqlHelper.ExecuteNonQueryAsync(updateQuery, parameters);

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> GetMaxofUserId()
        {
            try
            {
                string query = "SELECT count(1) FROM core.userM";
                DataTable result = await _sqlHelper.ExecuteQueryAsync(query);
                return result == null ? (int?)null : Convert.ToInt32(result.Rows[0][0]);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static User MapDataRowToUser(DataRow row)
        {
            return new User
            {
                id = Convert.ToInt64(row["id"]),
                userId = Convert.ToString(row["userId"]),
                userName = Convert.ToString(row["userName"]),
                userPassword = row["userPassword"].ToString(),
                panNo = row["panNo"].ToString(),
                adharCardNo = row["adharCardNo"].ToString(),
                phoneNo = row["phoneNo"].ToString(),
                address = row["address"].ToString(),
                stateId = row["stateId"] as int?,
                nationId = row["nationId"] as int?,
                isActive = Convert.ToBoolean(row["isActive"])
            };
        }
    }
}
