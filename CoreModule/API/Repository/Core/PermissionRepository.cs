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
    public class PermissionRepository : IPermissionRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public PermissionRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Permissions";
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
            var query = "SELECT * FROM core.Permissions WHERE PermissionID = @PermissionID";
            var parameters = new[] { new SqlParameter("@PermissionID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToPermission(dataTable.Rows[0]) : null;
        }

        public async Task<Permission> AddAsync(Permission permission)
        {
            var query = @"
                INSERT INTO core.Permissions (PermissionKey, Description, MenuItemID, ModuleID)
                OUTPUT INSERTED.*
                VALUES (@PermissionKey, @Description, @MenuItemID, @ModuleID);";

            var parameters = new[]
            {
                new SqlParameter("@PermissionKey", permission.PermissionKey),
                new SqlParameter("@Description", permission.Description),
                new SqlParameter("@MenuItemID", (object)permission.MenuItemID ?? DBNull.Value),
                new SqlParameter("@ModuleID", permission.ModuleID)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToPermission(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            var query = @"
                UPDATE core.Permissions
                SET PermissionKey = @PermissionKey, Description = @Description, MenuItemID = @MenuItemID, ModuleID = @ModuleID
                WHERE PermissionID = @PermissionID;";

            var parameters = new[]
            {
                new SqlParameter("@PermissionID", permission.PermissionID),
                new SqlParameter("@PermissionKey", permission.PermissionKey),
                new SqlParameter("@Description", permission.Description),
                new SqlParameter("@MenuItemID", (object)permission.MenuItemID ?? DBNull.Value),
                new SqlParameter("@ModuleID", permission.ModuleID)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Permissions WHERE PermissionID = @PermissionID";
            var parameters = new[] { new SqlParameter("@PermissionID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Permission MapToPermission(DataRow row)
        {
            return new Permission
            {
                PermissionID = Convert.ToInt32(row["PermissionID"]),
                PermissionKey = row["PermissionKey"].ToString(),
                Description = row["Description"].ToString(),
                MenuItemID = row["MenuItemID"] != DBNull.Value ? Convert.ToInt32(row["MenuItemID"]) : null,
                ModuleID = Convert.ToInt32(row["ModuleID"])
            };
        }
    }
}
