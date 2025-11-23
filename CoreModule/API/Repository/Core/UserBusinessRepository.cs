using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Entities.Core;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class UserBusinessRepository : IUserBusinessRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<UserBusinessRepository> _logger;

        public UserBusinessRepository(IDbConnectionFactory connectionFactory, ILogger<UserBusinessRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<UserBusiness>> GetByUserIdAsync(Guid userId)
        {
            var query = @"
                SELECT ub.user_business_id,
                       ub.user_id,
                       ub.business_id,
                       ub.is_default,
                       ub.created_at_utc,
                       ub.updated_at_utc,
                       b.business_name,
                       b.business_code
                FROM core.user_businesses ub
                INNER JOIN core.businesses b ON b.business_id = ub.business_id
                WHERE ub.user_id = @user_id
                ORDER BY ub.is_default DESC, ub.created_at_utc DESC;";

            var connection = _connectionFactory.CreateConnection();
            var npgsqlConnection = (NpgsqlConnection)connection;
            await npgsqlConnection.OpenAsync();
            
            using var command = new NpgsqlCommand(query, npgsqlConnection);
            command.Parameters.AddWithValue("@user_id", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<UserBusiness>();
            
            while (await reader.ReadAsync())
            {
                results.Add(MapUserBusiness(reader));
            }
            
            return results;
        }

        public async Task<UserBusiness> CreateAsync(Guid userId, Guid businessId, bool isDefault, Guid? createdByUserId)
        {
            // Use transaction to handle is_default logic
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // If setting as default, unset other defaults for this user
                if (isDefault)
                {
                    var unsetQuery = @"
                        UPDATE core.user_businesses 
                        SET is_default = FALSE, updated_at_utc = NOW() AT TIME ZONE 'UTC'
                        WHERE user_id = @user_id AND is_default = TRUE;";
                    
                    using var unsetCommand = new NpgsqlCommand(unsetQuery, connection, transaction);
                    unsetCommand.Parameters.AddWithValue("@user_id", userId);
                    await unsetCommand.ExecuteNonQueryAsync();
                }

                var insertQuery = @"
                    INSERT INTO core.user_businesses (user_id, business_id, is_default, created_at_utc)
                    VALUES (@user_id, @business_id, @is_default, NOW() AT TIME ZONE 'UTC')
                    RETURNING user_business_id;";
                
                using var insertCommand = new NpgsqlCommand(insertQuery, connection, transaction);
                insertCommand.Parameters.AddWithValue("@user_id", userId);
                insertCommand.Parameters.AddWithValue("@business_id", businessId);
                insertCommand.Parameters.AddWithValue("@is_default", isDefault);

                var insertedIdObj = await insertCommand.ExecuteScalarAsync();
                var insertedId = insertedIdObj switch
                {
                    Guid guidValue => guidValue,
                    string strValue when Guid.TryParse(strValue, out var parsedGuid) => parsedGuid,
                    byte[] bytesValue when bytesValue.Length == 16 => new Guid(bytesValue),
                    _ => (Guid?)null
                };

                if (!insertedId.HasValue)
                {
                    throw new InvalidOperationException("Failed to create user_business record");
                }

                await transaction.CommitAsync();
                await connection.CloseAsync();

                return await LoadUserBusinessAsync(insertedId.Value);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<UserBusiness> LoadUserBusinessAsync(Guid userBusinessId)
        {
            const string query = @"
                SELECT ub.user_business_id,
                       ub.user_id,
                       ub.business_id,
                       ub.is_default,
                       ub.created_at_utc,
                       ub.updated_at_utc,
                       b.business_name,
                       b.business_code
                FROM core.user_businesses ub
                INNER JOIN core.businesses b ON b.business_id = ub.business_id
                WHERE ub.user_business_id = @user_business_id;";

            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_business_id", userBusinessId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUserBusiness(reader);
            }

            throw new InvalidOperationException($"Failed to load user_business record for id {userBusinessId}");
        }

        public async Task<bool> SetDefaultAsync(Guid userBusinessId, Guid userId, Guid? updatedByUserId)
        {
            var connection = _connectionFactory.CreateConnection();
            var npgsqlConnection = (NpgsqlConnection)connection;
            await npgsqlConnection.OpenAsync();
            using var transaction = npgsqlConnection.BeginTransaction();
            
            try
            {
                // First, verify the user_business_id belongs to the user
                var verifyQuery = @"
                    SELECT user_id FROM core.user_businesses 
                    WHERE user_business_id = @user_business_id;";
                
                using var verifyCommand = new NpgsqlCommand(verifyQuery, npgsqlConnection, transaction);
                verifyCommand.Parameters.AddWithValue("@user_business_id", userBusinessId);
                
                var actualUserId = await verifyCommand.ExecuteScalarAsync();
                if (actualUserId == null || (Guid)actualUserId != userId)
                {
                    transaction.Rollback();
                    return false;
                }

                // Unset other defaults for this user
                var unsetQuery = @"
                    UPDATE core.user_businesses 
                    SET is_default = FALSE, updated_at_utc = NOW() AT TIME ZONE 'UTC'
                    WHERE user_id = @user_id AND is_default = TRUE;";
                
                using var unsetCommand = new NpgsqlCommand(unsetQuery, npgsqlConnection, transaction);
                unsetCommand.Parameters.AddWithValue("@user_id", userId);
                await unsetCommand.ExecuteNonQueryAsync();

                // Set this one as default
                var setQuery = @"
                    UPDATE core.user_businesses 
                    SET is_default = TRUE, updated_at_utc = NOW() AT TIME ZONE 'UTC'
                    WHERE user_business_id = @user_business_id;";
                
                using var setCommand = new NpgsqlCommand(setQuery, npgsqlConnection, transaction);
                setCommand.Parameters.AddWithValue("@user_business_id", userBusinessId);
                var rowsAffected = await setCommand.ExecuteNonQueryAsync();
                
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid userBusinessId)
        {
            var query = @"
                DELETE FROM core.user_businesses 
                WHERE user_business_id = @user_business_id;";

            var connection = _connectionFactory.CreateConnection();
            var npgsqlConnection = (NpgsqlConnection)connection;
            await npgsqlConnection.OpenAsync();
            
            using var command = new NpgsqlCommand(query, npgsqlConnection);
            command.Parameters.AddWithValue("@user_business_id", userBusinessId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<Guid?> GetDefaultBusinessIdAsync(Guid userId)
        {
            var query = @"
                SELECT business_id 
                FROM core.user_businesses 
                WHERE user_id = @user_id AND is_default = TRUE
                LIMIT 1;";

            var connection = _connectionFactory.CreateConnection();
            var npgsqlConnection = (NpgsqlConnection)connection;
            await npgsqlConnection.OpenAsync();
            
            using var command = new NpgsqlCommand(query, npgsqlConnection);
            command.Parameters.AddWithValue("@user_id", userId);
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? (Guid?)result : null;
        }

        private static UserBusiness MapUserBusiness(NpgsqlDataReader reader)
        {
            return new UserBusiness
            {
                UserBusinessID = reader.GetGuid("user_business_id"),
                UserID = reader.GetGuid("user_id"),
                BusinessID = reader.GetGuid("business_id"),
                IsDefault = reader.GetBoolean("is_default"),
                CreatedAtUTC = reader.GetDateTime("created_at_utc"),
                UpdatedAtUTC = reader.IsDBNull("updated_at_utc") ? null : reader.GetDateTime("updated_at_utc"),
                BusinessName = reader.IsDBNull("business_name") ? null : reader.GetString("business_name"),
                BusinessCode = reader.IsDBNull("business_code") ? null : reader.GetString("business_code")
            };
        }
    }
}

