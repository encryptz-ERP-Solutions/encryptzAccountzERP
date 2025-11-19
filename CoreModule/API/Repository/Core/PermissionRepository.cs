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
    public class PermissionRepository : IPermissionRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public PermissionRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            var query = "SELECT * FROM core.permissions";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var permissions = new List<Permission>();

            foreach (DataRow row in dataTable.Rows)
            {
                permissions.Add(MapToPermission(row));
            }

            return permissions;
        }

        public async Task<Permission> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.permissions WHERE permission_id = @PermissionID";
            var parameters = new[] { new NpgsqlParameter("@PermissionID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToPermission(dataTable.Rows[0]) : null;
        }

        public async Task<Permission> AddAsync(Permission permission)
        {
            var query = @"
                INSERT INTO core.permissions (permission_key, description, menu_item_id, module_id)
                VALUES (@PermissionKey, @Description, @MenuItemID, @ModuleID)
                RETURNING permission_id, permission_key, description, menu_item_id, module_id;";

            var parameters = new[]
            {
                new NpgsqlParameter("@PermissionKey", permission.PermissionKey),
                new NpgsqlParameter("@Description", permission.Description),
                new NpgsqlParameter("@MenuItemID", (object)permission.MenuItemID ?? DBNull.Value),
                new NpgsqlParameter("@ModuleID", permission.ModuleID)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToPermission(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            var query = @"
                UPDATE core.permissions
                SET permission_key = @PermissionKey, description = @Description, menu_item_id = @MenuItemID, module_id = @ModuleID
                WHERE permission_id = @PermissionID;";

            var parameters = new[]
            {
                new NpgsqlParameter("@PermissionID", permission.PermissionID),
                new NpgsqlParameter("@PermissionKey", permission.PermissionKey),
                new NpgsqlParameter("@Description", permission.Description),
                new NpgsqlParameter("@MenuItemID", (object)permission.MenuItemID ?? DBNull.Value),
                new NpgsqlParameter("@ModuleID", permission.ModuleID)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.permissions WHERE permission_id = @PermissionID";
            var parameters = new[] { new NpgsqlParameter("@PermissionID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Permission MapToPermission(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new Permission
            {
                PermissionID = Convert.ToInt32(row["permission_id"]),
                PermissionKey = row["permission_key"].ToString(),
                Description = row["description"].ToString(),
                MenuItemID = row["menu_item_id"] != DBNull.Value ? Convert.ToInt32(row["menu_item_id"]) : null,
                ModuleID = Convert.ToInt32(row["module_id"])
            };
        }
    }
}
