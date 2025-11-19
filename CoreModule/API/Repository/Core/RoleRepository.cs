using Entities.Core;
using Infrastructure;
using Npgsql;
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
                SELECT DISTINCT ubr.business_id, p.permission_key
                FROM core.users u
                JOIN core.user_business_roles ubr ON u.user_id = ubr.user_id
                JOIN core.roles r ON ubr.role_id = r.role_id
                JOIN core.role_permissions rp ON r.role_id = rp.role_id
                JOIN core.permissions p ON rp.permission_id = p.permission_id
                WHERE u.user_id = @UserID;";

            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var permissionsByBusiness = new Dictionary<Guid, List<string>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var businessId = (Guid)row["business_id"];
                var permissionKey = row["permission_key"].ToString();

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
            var query = "SELECT * FROM core.roles";
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
            var query = "SELECT * FROM core.roles WHERE role_id = @RoleID";
            var parameters = new[] { new NpgsqlParameter("@RoleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToRole(dataTable.Rows[0]) : null;
        }

        public async Task<Role> AddAsync(Role role)
        {
            var query = @"
                INSERT INTO core.roles (role_name, description, is_system_role)
                VALUES (@RoleName, @Description, @IsSystemRole)
                RETURNING role_id, role_name, description, is_system_role;";

            var parameters = new[]
            {
                new NpgsqlParameter("@RoleName", role.RoleName),
                new NpgsqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new NpgsqlParameter("@IsSystemRole", role.IsSystemRole)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToRole(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            var query = @"
                UPDATE core.roles
                SET role_name = @RoleName, description = @Description, is_system_role = @IsSystemRole
                WHERE role_id = @RoleID;";

            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", role.RoleID),
                new NpgsqlParameter("@RoleName", role.RoleName),
                new NpgsqlParameter("@Description", (object)role.Description ?? DBNull.Value),
                new NpgsqlParameter("@IsSystemRole", role.IsSystemRole)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.roles WHERE role_id = @RoleID";
            var parameters = new[] { new NpgsqlParameter("@RoleID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }
        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
        {
            var query = @"
                SELECT p.*
                FROM core.permissions p
                JOIN core.role_permissions rp ON p.permission_id = rp.permission_id
                WHERE rp.role_id = @RoleID;";
            var parameters = new[] { new NpgsqlParameter("@RoleID", roleId) };
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
            var query = "INSERT INTO core.role_permissions (role_id, permission_id) VALUES (@RoleID, @PermissionID)";
            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", roleId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var query = "DELETE FROM core.role_permissions WHERE role_id = @RoleID AND permission_id = @PermissionID";
            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", roleId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }


        private Role MapToRole(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new Role
            {
                RoleID = Convert.ToInt32(row["role_id"]),
                RoleName = row["role_name"].ToString(),
                Description = row["description"] != DBNull.Value ? row["description"].ToString() : null,
                IsSystemRole = Convert.ToBoolean(row["is_system_role"])
            };
        }

        private Permission MapToPermission(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new Permission
            {
                PermissionID = Convert.ToInt32(row["permission_id"]),
                PermissionKey = row["permission_key"].ToString(),
                Description = row["description"].ToString(),
                MenuItemID = row["menu_item_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["menu_item_id"]),
                ModuleID = Convert.ToInt32(row["module_id"])
            };
        }
    }
}
