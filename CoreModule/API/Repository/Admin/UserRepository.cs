using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Data.Core;
using Entities.Admin;
using Microsoft.Data.SqlClient;
using Repository.Admin.Interface;

namespace Repository.Admin
{
    public class UserRepository : IUserRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseUserSelectQuery = "SELECT UserID, UserHandle, FullName, Email, HashedPassword, MobileCountryCode, MobileNumber, PanCardNumber_Encrypted, AadharNumber_Encrypted, IsActive, CreatedAtUTC, UpdatedAtUTC FROM Admin.Users";

        public UserRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var dataTable = await _sqlHelper.ExecuteQueryAsync(BaseUserSelectQuery, null);
            var users = new List<User>();
            foreach (DataRow row in dataTable.Rows)
            {
                users.Add(MapDataRowToUser(row));
            }
            return users;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var query = $"{BaseUserSelectQuery} WHERE UserID = @UserID";
            var parameters = new[] { new SqlParameter("@UserID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User?> GetByUserHandleAsync(string userHandle)
        {
            var query = $"{BaseUserSelectQuery} WHERE UserHandle = @UserHandle";
            var parameters = new[] { new SqlParameter("@UserHandle", userHandle) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var query = $"{BaseUserSelectQuery} WHERE Email = @Email";
            var parameters = new[] { new SqlParameter("@Email", email) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User> AddAsync(User user)
        {
            var query = @"
                INSERT INTO Admin.Users (UserID, UserHandle, FullName, Email, HashedPassword, MobileCountryCode, MobileNumber, PanCardNumber_Encrypted, AadharNumber_Encrypted, IsActive, CreatedAtUTC, UpdatedAtUTC)
                VALUES (@UserID, @UserHandle, @FullName, @Email, @HashedPassword, @MobileCountryCode, @MobileNumber, @PanCardNumber_Encrypted, @AadharNumber_Encrypted, @IsActive, @CreatedAtUTC, @UpdatedAtUTC);

                SELECT * FROM Admin.Users WHERE UserID = @UserID;";

            var parameters = GetSqlParameters(user);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to add user, SELECT query returned no results.");
            }

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User> UpdateAsync(User user)
        {
            var query = @"
                UPDATE Admin.Users SET
                    UserHandle = @UserHandle,
                    FullName = @FullName,
                    Email = @Email,
                    HashedPassword = @HashedPassword,
                    MobileCountryCode = @MobileCountryCode,
                    MobileNumber = @MobileNumber,
                    PanCardNumber_Encrypted = @PanCardNumber_Encrypted,
                    AadharNumber_Encrypted = @AadharNumber_Encrypted,
                    IsActive = @IsActive,
                    UpdatedAtUTC = @UpdatedAtUTC
                WHERE UserID = @UserID;

                SELECT * FROM Admin.Users WHERE UserID = @UserID;";

            var parameters = GetSqlParameters(user);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to update user, user may not exist.");
            }

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task DeleteAsync(Guid id)
        {
            var query = "DELETE FROM Admin.Users WHERE UserID = @UserID";
            var parameters = new[] { new SqlParameter("@UserID", id) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private static User MapDataRowToUser(DataRow row)
        {
            return new User
            {
                UserID = row.Field<Guid>("UserID"),
                UserHandle = row.Field<string>("UserHandle") ?? string.Empty,
                FullName = row.Field<string>("FullName") ?? string.Empty,
                Email = row.Field<string?>("Email"),
                HashedPassword = row.Field<string?>("HashedPassword"),
                MobileCountryCode = row.Field<string?>("MobileCountryCode"),
                MobileNumber = row.Field<string?>("MobileNumber"),
                PanCardNumber_Encrypted = row.Field<byte[]>("PanCardNumber_Encrypted") ?? Array.Empty<byte>(),
                AadharNumber_Encrypted = row.Field<byte[]?>("AadharNumber_Encrypted"),
                IsActive = row.Field<bool>("IsActive"),
                CreatedAtUTC = row.Field<DateTime>("CreatedAtUTC"),
                UpdatedAtUTC = row.Field<DateTime>("UpdatedAtUTC")
            };
        }

        private static SqlParameter[] GetSqlParameters(User user)
        {
            return new[]
            {
                new SqlParameter("@UserID", user.UserID),
                new SqlParameter("@UserHandle", user.UserHandle),
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", (object)user.Email ?? DBNull.Value),
                new SqlParameter("@HashedPassword", (object)user.HashedPassword ?? DBNull.Value),
                new SqlParameter("@MobileCountryCode", (object)user.MobileCountryCode ?? DBNull.Value),
                new SqlParameter("@MobileNumber", (object)user.MobileNumber ?? DBNull.Value),
                new SqlParameter("@PanCardNumber_Encrypted", user.PanCardNumber_Encrypted),
                new SqlParameter("@AadharNumber_Encrypted", (object)user.AadharNumber_Encrypted ?? DBNull.Value),
                new SqlParameter("@IsActive", user.IsActive),
                new SqlParameter("@CreatedAtUTC", user.CreatedAtUTC),
                new SqlParameter("@UpdatedAtUTC", user.UpdatedAtUTC)
            };
        }
    }
}