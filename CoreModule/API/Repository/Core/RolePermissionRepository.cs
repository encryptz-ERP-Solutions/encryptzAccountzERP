using Entities.Core;
using Repository.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
            var sql = "INSERT INTO core.RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", rolePermission.RoleID),
                new SqlParameter("@PermissionID", rolePermission.PermissionID)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteAsync(int roleId, int permissionId)
        {
            var sql = "DELETE FROM core.RolePermissions WHERE RoleID = @RoleID AND PermissionID = @PermissionID";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@PermissionID", permissionId)
            };
            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<RolePermission>> GetAllAsync()
        {
            var sql = "SELECT * FROM core.RolePermissions";
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql);
            var rolePermissions = new List<RolePermission>();
            foreach (DataRow row in dataTable.Rows)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleID = Convert.ToInt32(row["RoleID"]),
                    PermissionID = Convert.ToInt32(row["PermissionID"])
                });
            }
            return rolePermissions;
        }

        public async Task<RolePermission> GetByIdAsync(int roleId, int permissionId)
        {
            var sql = "SELECT * FROM core.RolePermissions WHERE RoleID = @RoleID AND PermissionID = @PermissionID";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@PermissionID", permissionId)
            };
            var dataTable = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return new RolePermission
                {
                    RoleID = Convert.ToInt32(row["RoleID"]),
                    PermissionID = Convert.ToInt32(row["PermissionID"])
                };
            }
            return null;
        }
    }
}
