using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Core;
using Entities.Core;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;

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
            const string query = "SELECT RoleID, RoleName, Description, IsSystemRole FROM core.Roles";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, null);
            var roles = new List<Role>();
            foreach (DataRow row in dataTable.Rows)
            {
                roles.Add(MapDataRowToRole(row));
            }
            return roles;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            const string query = "SELECT RoleID, RoleName, Description, IsSystemRole FROM core.Roles WHERE RoleID = @RoleID";
            var parameters = new[] { new SqlParameter("@RoleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToRole(dataTable.Rows[0]);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            const string query = @"
                SELECT p.PermissionID, p.PermissionKey, p.Description, p.ModuleID
                FROM core.Permissions p
                INNER JOIN core.RolePermissions rp ON p.PermissionID = rp.PermissionID
                WHERE rp.RoleID = @RoleID";

            var parameters = new[] { new SqlParameter("@RoleID", roleId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            var permissions = new List<Permission>();
            foreach (DataRow row in dataTable.Rows)
            {
                permissions.Add(new Permission
                {
                    PermissionID = row.Field<int>("PermissionID"),
                    PermissionKey = row.Field<string>("PermissionKey") ?? string.Empty,
                    Description = row.Field<string>("Description") ?? string.Empty,
                    ModuleID = row.Field<int>("ModuleID")
                });
            }
            return permissions;
        }

        public async Task<Role> AddAsync(Role role, IEnumerable<int> permissionIds)
        {
            var insertRoleQuery = @"
                INSERT INTO core.Roles (RoleName, Description, IsSystemRole)
                VALUES (@RoleName, @Description, @IsSystemRole);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var roleParams = new[]
            {
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@IsSystemRole", role.IsSystemRole)
            };

            var newRoleId = await _sqlHelper.ExecuteScalarAsync(insertRoleQuery, roleParams);
            role.RoleID = (int)newRoleId;

            await AssignPermissionsToRole(role.RoleID, permissionIds);

            return role;
        }

        public async Task<Role> UpdateAsync(Role role, IEnumerable<int> permissionIds)
        {
            var updateRoleQuery = @"
                UPDATE core.Roles SET
                    RoleName = @RoleName,
                    Description = @Description
                WHERE RoleID = @RoleID";

            var roleParams = new[]
            {
                new SqlParameter("@RoleName", role.RoleName),
                new SqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new SqlParameter("@RoleID", role.RoleID)
            };

            await _sqlHelper.ExecuteNonQueryAsync(updateRoleQuery, roleParams);

            // Re-assign permissions (delete old, insert new)
            await AssignPermissionsToRole(role.RoleID, permissionIds);

            return role;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // The database schema should have cascading deletes on the RolePermissions table.
            const string query = "DELETE FROM core.Roles WHERE RoleID = @RoleID AND IsSystemRole = 0";
            var parameters = new[] { new SqlParameter("@RoleID", id) };
            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private async Task AssignPermissionsToRole(int roleId, IEnumerable<int> permissionIds)
        {
            // 1. Delete existing permissions for the role
            var deleteQuery = "DELETE FROM core.RolePermissions WHERE RoleID = @RoleID";
            await _sqlHelper.ExecuteNonQueryAsync(deleteQuery, new[] { new SqlParameter("@RoleID", roleId) });

            // 2. Insert new permissions
            if (permissionIds != null && permissionIds.Any())
            {
                var insertQuery = new StringBuilder("INSERT INTO core.RolePermissions (RoleID, PermissionID) VALUES ");
                var parameters = new List<SqlParameter> { new SqlParameter("@RoleID", roleId) };

                int i = 0;
                foreach (var permId in permissionIds)
                {
                    var paramName = $"@PermissionID{i}";
                    insertQuery.Append($"(@RoleID, {paramName}),");
                    parameters.Add(new SqlParameter(paramName, permId));
                    i++;
                }

                // Remove trailing comma and execute
                insertQuery.Length--;
                await _sqlHelper.ExecuteNonQueryAsync(insertQuery.ToString(), parameters.ToArray());
            }
        }

        private static Role MapDataRowToRole(DataRow row)
        {
            return new Role
            {
                RoleID = row.Field<int>("RoleID"),
                RoleName = row.Field<string>("RoleName") ?? string.Empty,
                Description = row.Field<string?>("Description"),
                IsSystemRole = row.Field<bool>("IsSystemRole")
            };
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid businessId, int roleId)
        {
            // First, remove any existing role for this user in this business to ensure they only have one.
            var deleteQuery = "DELETE FROM core.UserBusinessRoles WHERE UserID = @UserID AND BusinessID = @BusinessID";
            var deleteParams = new[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@BusinessID", businessId)
            };
            await _sqlHelper.ExecuteNonQueryAsync(deleteQuery, deleteParams);

            // Now, insert the new role assignment.
            var insertQuery = "INSERT INTO core.UserBusinessRoles (UserID, BusinessID, RoleID) VALUES (@UserID, @BusinessID, @RoleID)";
            var insertParams = new[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@BusinessID", businessId),
                new SqlParameter("@RoleID", roleId)
            };
            await _sqlHelper.ExecuteNonQueryAsync(insertQuery, insertParams);
        }

        public async Task<Dictionary<Guid, IEnumerable<string>>> GetUserPermissionsAcrossBusinessesAsync(Guid userId)
        {
            const string query = @"
                SELECT
                    ubr.BusinessID,
                    p.PermissionKey
                FROM core.UserBusinessRoles ubr
                JOIN core.RolePermissions rp ON ubr.RoleID = rp.RoleID
                JOIN core.Permissions p ON rp.PermissionID = p.PermissionID
                WHERE ubr.UserID = @UserID";

            var parameters = new[] { new SqlParameter("@UserID", userId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            var permissionsByBusiness = dataTable.AsEnumerable()
                .GroupBy(row => row.Field<Guid>("BusinessID"))
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(row => row.Field<string>("PermissionKey") ?? string.Empty).Where(pk => !string.IsNullOrEmpty(pk))
                );

            return permissionsByBusiness.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
        }
    }
}