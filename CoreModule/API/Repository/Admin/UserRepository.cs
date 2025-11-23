using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Entities.Admin;
using Infrastructure;
using Npgsql;
using Repository.Admin.Interface;

namespace Repository.Admin
{
    public class UserRepository : IUserRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseUserSelectQuery = "SELECT user_id, user_handle, full_name, email, hashed_password, mobile_country_code, mobile_number, pan_card_number_encrypted, aadhar_number_encrypted, is_active, created_at_utc, updated_at_utc FROM core.users";

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
            var query = $"{BaseUserSelectQuery} WHERE user_id = @UserID";
            var parameters = new[] { new NpgsqlParameter("@UserID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User?> GetByUserHandleAsync(string userHandle)
        {
            var query = $"{BaseUserSelectQuery} WHERE user_handle = @UserHandle";
            var parameters = new[] { new NpgsqlParameter("@UserHandle", userHandle) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var query = $"{BaseUserSelectQuery} WHERE email = @Email";
            var parameters = new[] { new NpgsqlParameter("@Email", email) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToUser(dataTable.Rows[0]);
        }

        public async Task<User> AddAsync(User user)
        {
            var query = @"
                INSERT INTO core.users (user_id, user_handle, full_name, email, hashed_password, mobile_country_code, mobile_number, pan_card_number_encrypted, aadhar_number_encrypted, is_active, created_at_utc, updated_at_utc)
                VALUES (@UserID, @UserHandle, @FullName, @Email, @HashedPassword, @MobileCountryCode, @MobileNumber, @PanCardNumber_Encrypted, @AadharNumber_Encrypted, @IsActive, @CreatedAtUTC, @UpdatedAtUTC)
                RETURNING user_id, user_handle, full_name, email, hashed_password, mobile_country_code, mobile_number, pan_card_number_encrypted, aadhar_number_encrypted, is_active, created_at_utc, updated_at_utc;";

            var parameters = GetSqlParameters(user);
            
            try
            {
                var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new DataException("Failed to add user, RETURNING query returned no results.");
                }

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique constraint violation
            {
                // Provide more specific error messages based on the constraint name
                if (ex.ConstraintName?.Contains("pan", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this PAN card number already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("email", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this email already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("user_handle", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this handle already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("mobile", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this mobile number already exists.", ex);
                }
                
                throw new InvalidOperationException("A user with this information already exists.", ex);
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            var query = @"
                UPDATE core.users SET
                    user_handle = @UserHandle,
                    full_name = @FullName,
                    email = @Email,
                    hashed_password = @HashedPassword,
                    mobile_country_code = @MobileCountryCode,
                    mobile_number = @MobileNumber,
                    pan_card_number_encrypted = @PanCardNumber_Encrypted,
                    aadhar_number_encrypted = @AadharNumber_Encrypted,
                    is_active = @IsActive,
                    updated_at_utc = @UpdatedAtUTC
                WHERE user_id = @UserID
                RETURNING user_id, user_handle, full_name, email, hashed_password, mobile_country_code, mobile_number, pan_card_number_encrypted, aadhar_number_encrypted, is_active, created_at_utc, updated_at_utc;";

            var parameters = GetSqlParameters(user);
            
            try
            {
                var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new DataException("Failed to update user, user may not exist.");
                }

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique constraint violation
            {
                // Provide more specific error messages based on the constraint name
                if (ex.ConstraintName?.Contains("pan", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this PAN card number already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("email", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this email already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("user_handle", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this handle already exists.", ex);
                }
                else if (ex.ConstraintName?.Contains("mobile", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("A user with this mobile number already exists.", ex);
                }
                
                throw new InvalidOperationException("A user with this information already exists.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var query = "DELETE FROM core.users WHERE user_id = @UserID";
            var parameters = new[] { new NpgsqlParameter("@UserID", id) };
            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private static User MapDataRowToUser(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new User
            {
                UserID = row.Field<Guid>("user_id"),
                UserHandle = row.Field<string>("user_handle") ?? string.Empty,
                FullName = row.Field<string>("full_name") ?? string.Empty,
                Email = row.Field<string?>("email"),
                HashedPassword = row.Field<string?>("hashed_password"),
                MobileCountryCode = row.Field<string?>("mobile_country_code"),
                MobileNumber = row.Field<string?>("mobile_number"),
                PanCardNumber_Encrypted = row.Field<byte[]?>("pan_card_number_encrypted"),
                AadharNumber_Encrypted = row.Field<byte[]?>("aadhar_number_encrypted"),
                IsActive = row.Field<bool>("is_active"),
                CreatedAtUTC = row.Field<DateTime>("created_at_utc"),
                UpdatedAtUTC = row.Field<DateTime?>("updated_at_utc")
            };
        }

        private static NpgsqlParameter[] GetSqlParameters(User user)
        {
            // Convert HashedPassword string to byte array for varbinary storage
            

            return new[]
            {
                new NpgsqlParameter("@UserID", user.UserID),
                new NpgsqlParameter("@UserHandle", user.UserHandle),
                new NpgsqlParameter("@FullName", user.FullName),
                new NpgsqlParameter("@Email", (object)user.Email ?? DBNull.Value),
                new NpgsqlParameter("@HashedPassword", user.HashedPassword),
                new NpgsqlParameter("@MobileCountryCode", (object)user.MobileCountryCode ?? DBNull.Value),
                new NpgsqlParameter("@MobileNumber", (object)user.MobileNumber ?? DBNull.Value),
                new NpgsqlParameter("@PanCardNumber_Encrypted", (object)user.PanCardNumber_Encrypted ?? DBNull.Value),
                new NpgsqlParameter("@AadharNumber_Encrypted", (object)user.AadharNumber_Encrypted ?? DBNull.Value),
                new NpgsqlParameter("@IsActive", user.IsActive),
                new NpgsqlParameter("@CreatedAtUTC", user.CreatedAtUTC),
                new NpgsqlParameter("@UpdatedAtUTC", (object)user.UpdatedAtUTC ?? DBNull.Value)
            };
        }
    }
}