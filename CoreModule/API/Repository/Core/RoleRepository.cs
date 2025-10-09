using Data.Core;
using Entities.Core;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repository.Core
{
    public class RoleRepository : IRoleRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public RoleRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Roles";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var roles = new List<Role>();

            foreach (DataRow row in dataTable.Rows)
            {
                roles.Add(MapToRole(row));
            }

            return roles;
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.Roles WHERE RoleID = @RoleID";
            var parameters = new[] { new SqlParameter("@RoleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToRole(dataTable.Rows[0]) : null;
        }

        public async Task<Role> AddAsync(Role role)
        {
            var query = @"
                INSERT INTO core.Roles (RoleName, Description, IsSystemRole)
                OUTPUT INSERTED.*
                VALUES (@RoleName, @Description, @IsSystemRole);";

            var parameters = new[]
            {
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@IsSystemRole", role.IsSystemRole)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToRole(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            var query = @"
                UPDATE core.Roles
                SET RoleName = @RoleName, Description = @Description, IsSystemRole = @IsSystemRole
                WHERE RoleID = @RoleID;";

            var parameters = new[]
            {
                new SqlParameter("@RoleID", role.RoleID),
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@IsSystemRole", role.IsSystemRole)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Roles WHERE RoleID = @RoleID";
            var parameters = new[] { new SqlParameter("@RoleID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Role MapToRole(DataRow row)
        {
            return new Role
            {
                RoleID = Convert.ToInt32(row["RoleID"]),
                RoleName = row["RoleName"].ToString(),
                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                IsSystemRole = Convert.ToBoolean(row["IsSystemRole"])
            };
        }
    }
}
