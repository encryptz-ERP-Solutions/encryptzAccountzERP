using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            var userBusinessRoles = new List<UserBusinessRole>();
            foreach (DataRow row in dataTable.Rows)
            {
                userBusinessRoles.Add(new UserBusinessRole
                {
                    UserID = (Guid)row["UserID"],
                    BusinessID = (Guid)row["BusinessID"],
                    RoleID = Convert.ToInt32(row["RoleID"])
                });
            }
            return userBusinessRoles;
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
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return new UserBusinessRole
                {
                    UserID = (Guid)row["UserID"],
                    BusinessID = (Guid)row["BusinessID"],
                    RoleID = Convert.ToInt32(row["RoleID"])
                };
            }
            return null;
        }
    }
}
