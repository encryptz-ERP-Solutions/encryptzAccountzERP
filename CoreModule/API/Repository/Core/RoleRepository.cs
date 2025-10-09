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

        public async Task<Dictionary<Guid, IEnumerable<string>>> GetUserPermissionsAcrossBusinessesAsync(Guid userId)
        {
            var query = @"
                SELECT DISTINCT ubr.BusinessID, p.PermissionKey
                FROM core.Users u
                JOIN core.UserBusinessRoles ubr ON u.UserID = ubr.UserID
                JOIN core.Roles r ON ubr.RoleID = r.RoleID
                JOIN core.RolePermissions rp ON r.RoleID = rp.RoleID
                JOIN core.Permissions p ON rp.PermissionID = p.PermissionID
                WHERE u.UserID = @UserID;";

            var parameters = new[] { new SqlParameter("@UserID", userId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var permissionsByBusiness = new Dictionary<Guid, List<string>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var businessId = (Guid)row["BusinessID"];
                var permissionKey = row["PermissionKey"].ToString();

                if (!permissionsByBusiness.TryGetValue(businessId, out var permissions))
                {
                    permissions = new List<string>();
                    permissionsByBusiness[businessId] = permissions;
                }
                permissions.Add(permissionKey);
            }

            return permissionsByBusiness.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);
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
        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
        {
            var query = @"
                SELECT p.*
                FROM core.Permissions p
                JOIN core.RolePermissions rp ON p.PermissionID = rp.PermissionID
                WHERE rp.RoleID = @RoleID;";
            var parameters = new[] { new SqlParameter("@RoleID", roleId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var permissions = new List<Permission>();

            foreach (DataRow row in dataTable.Rows)
            {
                permissions.Add(MapToPermission(row));
            }

            return permissions;
        }

        public async Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId)
        {
            var query = "INSERT INTO core.RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@PermissionID", permissionId)
            };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var query = "DELETE FROM core.RolePermissions WHERE RoleID = @RoleID AND PermissionID = @PermissionID";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@PermissionID", permissionId)
            };
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

        private Permission MapToPermission(DataRow row)
        {
            return new Permission
            {
                PermissionID = Convert.ToInt32(row["PermissionID"]),
                PermissionKey = row["PermissionKey"].ToString(),
                Description = row["Description"].ToString(),
                MenuItemID = row["MenuItemID"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["MenuItemID"]),
                ModuleID = Convert.ToInt32(row["ModuleID"])
            };
        }
    }
}
