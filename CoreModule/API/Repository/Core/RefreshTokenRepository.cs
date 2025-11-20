using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Core;
using Infrastructure;
using Npgsql;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseSelectQuery = @"
            SELECT refresh_token_id, user_id, token_hash, expires_at_utc, created_at_utc, 
                   is_revoked, revoked_at_utc, replaced_by_token_id, created_by_ip, revoked_by_ip
            FROM core.refresh_tokens";

        public RefreshTokenRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            var query = $"{BaseSelectQuery} WHERE token_hash = @TokenHash";
            var parameters = new[] { new NpgsqlParameter("@TokenHash", tokenHash) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToRefreshToken(dataTable.Rows[0]);
        }

        public async Task<RefreshToken?> GetByIdAsync(Guid refreshTokenId)
        {
            var query = $"{BaseSelectQuery} WHERE refresh_token_id = @RefreshTokenID";
            var parameters = new[] { new NpgsqlParameter("@RefreshTokenID", refreshTokenId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToRefreshToken(dataTable.Rows[0]);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            var query = $"{BaseSelectQuery} WHERE user_id = @UserID AND is_revoked = false AND expires_at_utc > @Now";
            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@Now", DateTime.UtcNow)
            };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            var tokens = new List<RefreshToken>();
            foreach (DataRow row in dataTable.Rows)
            {
                tokens.Add(MapDataRowToRefreshToken(row));
            }
            return tokens;
        }

        public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
        {
            var query = @"
                INSERT INTO core.refresh_tokens 
                (refresh_token_id, user_id, token_hash, expires_at_utc, created_at_utc, 
                 is_revoked, revoked_at_utc, replaced_by_token_id, created_by_ip, revoked_by_ip)
                VALUES (@RefreshTokenID, @UserID, @TokenHash, @ExpiresAtUTC, @CreatedAtUTC, 
                        @IsRevoked, @RevokedAtUTC, @ReplacedByTokenID, @CreatedByIP, @RevokedByIP)
                RETURNING refresh_token_id, user_id, token_hash, expires_at_utc, created_at_utc, 
                          is_revoked, revoked_at_utc, replaced_by_token_id, created_by_ip, revoked_by_ip;";

            var parameters = GetSqlParameters(refreshToken);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to add refresh token, RETURNING query returned no results.");
            }

            return MapDataRowToRefreshToken(dataTable.Rows[0]);
        }

        public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
        {
            var query = @"
                UPDATE core.refresh_tokens SET
                    user_id = @UserID,
                    token_hash = @TokenHash,
                    expires_at_utc = @ExpiresAtUTC,
                    is_revoked = @IsRevoked,
                    revoked_at_utc = @RevokedAtUTC,
                    replaced_by_token_id = @ReplacedByTokenID,
                    revoked_by_ip = @RevokedByIP
                WHERE refresh_token_id = @RefreshTokenID
                RETURNING refresh_token_id, user_id, token_hash, expires_at_utc, created_at_utc, 
                          is_revoked, revoked_at_utc, replaced_by_token_id, created_by_ip, revoked_by_ip;";

            var parameters = GetSqlParameters(refreshToken);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to update refresh token, token may not exist.");
            }

            return MapDataRowToRefreshToken(dataTable.Rows[0]);
        }

        public async Task<bool> RevokeTokenAsync(Guid refreshTokenId, string? revokedByIP = null)
        {
            var query = @"
                UPDATE core.refresh_tokens 
                SET is_revoked = true, 
                    revoked_at_utc = @RevokedAtUTC,
                    revoked_by_ip = @RevokedByIP
                WHERE refresh_token_id = @RefreshTokenID";

            var parameters = new[]
            {
                new NpgsqlParameter("@RefreshTokenID", refreshTokenId),
                new NpgsqlParameter("@RevokedAtUTC", DateTime.UtcNow),
                new NpgsqlParameter("@RevokedByIP", (object?)revokedByIP ?? DBNull.Value)
            };

            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
        {
            var query = @"
                UPDATE core.refresh_tokens 
                SET is_revoked = true, 
                    revoked_at_utc = @RevokedAtUTC
                WHERE user_id = @UserID AND is_revoked = false";

            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@RevokedAtUTC", DateTime.UtcNow)
            };

            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<int> DeleteExpiredTokensAsync()
        {
            var query = @"
                DELETE FROM core.refresh_tokens 
                WHERE expires_at_utc < @Now OR (is_revoked = true AND revoked_at_utc < @OldDate)";

            var parameters = new[]
            {
                new NpgsqlParameter("@Now", DateTime.UtcNow),
                new NpgsqlParameter("@OldDate", DateTime.UtcNow.AddDays(-30)) // Keep revoked tokens for 30 days
            };

            return await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private static RefreshToken MapDataRowToRefreshToken(DataRow row)
        {
            return new RefreshToken
            {
                RefreshTokenID = row.Field<Guid>("refresh_token_id"),
                UserID = row.Field<Guid>("user_id"),
                TokenHash = row.Field<string>("token_hash") ?? string.Empty,
                ExpiresAtUTC = row.Field<DateTime>("expires_at_utc"),
                CreatedAtUTC = row.Field<DateTime>("created_at_utc"),
                IsRevoked = row.Field<bool>("is_revoked"),
                RevokedAtUTC = row.Field<DateTime?>("revoked_at_utc"),
                ReplacedByTokenID = row.Field<Guid?>("replaced_by_token_id"),
                CreatedByIP = row.Field<string?>("created_by_ip"),
                RevokedByIP = row.Field<string?>("revoked_by_ip")
            };
        }

        private static NpgsqlParameter[] GetSqlParameters(RefreshToken token)
        {
            return new[]
            {
                new NpgsqlParameter("@RefreshTokenID", token.RefreshTokenID),
                new NpgsqlParameter("@UserID", token.UserID),
                new NpgsqlParameter("@TokenHash", token.TokenHash),
                new NpgsqlParameter("@ExpiresAtUTC", token.ExpiresAtUTC),
                new NpgsqlParameter("@CreatedAtUTC", token.CreatedAtUTC),
                new NpgsqlParameter("@IsRevoked", token.IsRevoked),
                new NpgsqlParameter("@RevokedAtUTC", (object?)token.RevokedAtUTC ?? DBNull.Value),
                new NpgsqlParameter("@ReplacedByTokenID", (object?)token.ReplacedByTokenID ?? DBNull.Value),
                new NpgsqlParameter("@CreatedByIP", (object?)token.CreatedByIP ?? DBNull.Value),
                new NpgsqlParameter("@RevokedByIP", (object?)token.RevokedByIP ?? DBNull.Value)
            };
        }
    }
}

