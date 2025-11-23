using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Infrastructure;
using System.Data;

namespace Repository.Core
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
            var sql = "INSERT INTO core.user_business_roles (user_id, business_id, role_id) VALUES (@UserID, @BusinessID, @RoleID)";
            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userBusinessRole.UserID),
                new NpgsqlParameter("@BusinessID", userBusinessRole.BusinessID),
                new NpgsqlParameter("@RoleID", userBusinessRole.RoleID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteAsync(Guid userId, Guid businessId, int roleId)
        {
            var sql = "DELETE FROM core.user_business_roles WHERE user_id = @UserID AND business_id = @BusinessID AND role_id = @RoleID";
            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@BusinessID", businessId),
                new NpgsqlParameter("@RoleID", roleId)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<UserBusinessRole>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.user_business_roles";
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            var userBusinessRoles = new List<UserBusinessRole>();
            foreach (DataRow row in dataTable.Rows)
            {
                userBusinessRoles.Add(new UserBusinessRole
                {
                    UserID = (Guid)row["user_id"],
                    BusinessID = (Guid)row["business_id"],
                    RoleID = Convert.ToInt32(row["role_id"])
                });
            }
            return userBusinessRoles;
        }

        public async Task<UserBusinessRole> GetByIdAsync(Guid userId, Guid businessId, int roleId)
        {
            var sql = "SELECT * FROM core.user_business_roles WHERE user_id = @UserID AND business_id = @BusinessID AND role_id = @RoleID";
            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@BusinessID", businessId),
                new NpgsqlParameter("@RoleID", roleId)
            };
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return new UserBusinessRole
                {
                    UserID = (Guid)row["user_id"],
                    BusinessID = (Guid)row["business_id"],
                    RoleID = Convert.ToInt32(row["role_id"])
                };
            }
            return null;
        }
    }
}
