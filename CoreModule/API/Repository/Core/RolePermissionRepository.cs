using Entities.Core;
using Repository.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Infrastructure;
using System.Data;
using System;

namespace Repository.Core
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly CoreSQLDbHelper _dbHelper;

        public RolePermissionRepository(CoreSQLDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task AddAsync(RolePermission rolePermission)
        {
            var sql = "INSERT INTO core.role_permissions (role_id, permission_id) VALUES (@RoleID, @PermissionID)";
            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", rolePermission.RoleID),
                new NpgsqlParameter("@PermissionID", rolePermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteAsync(int roleId, int permissionId)
        {
            var sql = "DELETE FROM core.role_permissions WHERE role_id = @RoleID AND permission_id = @PermissionID";
            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", roleId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<RolePermission>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.role_permissions";
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            var rolePermissions = new List<RolePermission>();
            foreach (DataRow row in dataTable.Rows)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleID = Convert.ToInt32(row["role_id"]),
                    PermissionID = Convert.ToInt32(row["permission_id"])
                });
            }
            return rolePermissions;
        }

        public async Task<RolePermission> GetByIdAsync(int roleId, int permissionId)
        {
            var sql = "SELECT * FROM core.role_permissions WHERE role_id = @RoleID AND permission_id = @PermissionID";
            var parameters = new[]
            {
                new NpgsqlParameter("@RoleID", roleId),
                new NpgsqlParameter("@PermissionID", permissionId)
            };
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return new RolePermission
                {
                    RoleID = Convert.ToInt32(row["role_id"]),
                    PermissionID = Convert.ToInt32(row["permission_id"])
                };
            }
            return null;
        }
    }
}
