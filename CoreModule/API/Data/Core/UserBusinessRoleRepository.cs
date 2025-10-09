using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Infrastructure;
using System.Linq;

namespace Data.Core
{
    public class UserBusinessRoleRepository : IUserBusinessRoleRepository
    {
        private readonly CoreSQLDbHelper _dbHelper;

        public UserBusinessRoleRepository(CoreSQLDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task AddAsync(UserBusinessRole userBusinessRole)
        {
            var sql = "INSERT INTO core.UserBusinessRoles (UserID, BusinessID, RoleID) VALUES (@UserID, @BusinessID, @RoleID)";
            var parameters = new[]
            {
                new SqlParameter("@UserID", userBusinessRole.UserID),
                new SqlParameter("@BusinessID", userBusinessRole.BusinessID),
                new SqlParameter("@RoleID", userBusinessRole.RoleID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteAsync(Guid userId, Guid businessId, int roleId)
        {
            var sql = "DELETE FROM core.UserBusinessRoles WHERE UserID = @UserID AND BusinessID = @BusinessID AND RoleID = @RoleID";
            var parameters = new[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@BusinessID", businessId),
                new SqlParameter("@RoleID", roleId)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<UserBusinessRole>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.UserBusinessRoles";
            return await _dbHelper.ExecuteQueryAsync<UserBusinessRole>(sql);
        }

        public async Task<UserBusinessRole> GetByIdAsync(Guid userId, Guid businessId, int roleId)
        {
            var sql = "SELECT * FROM core.UserBusinessRoles WHERE UserID = @UserID AND BusinessID = @BusinessID AND RoleID = @RoleID";
            var parameters = new[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@BusinessID", businessId),
                new SqlParameter("@RoleID", roleId)
            };
            var result = await _dbHelper.ExecuteQueryAsync<UserBusinessRole>(sql, parameters);
            return result.FirstOrDefault();
        }
    }
}
