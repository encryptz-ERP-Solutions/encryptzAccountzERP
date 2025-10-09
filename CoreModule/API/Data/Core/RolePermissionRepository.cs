using Entities.Core;
using Repository.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Infrastructure;
using System.Linq;

namespace Data.Core
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
            return await _dbHelper.ExecuteQueryAsync<RolePermission>(sql);
        }

        public async Task<RolePermission> GetByIdAsync(int roleId, int permissionId)
        {
            var sql = "SELECT * FROM core.RolePermissions WHERE RoleID = @RoleID AND PermissionID = @PermissionID";
            var parameters = new[]
            {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@PermissionID", permissionId)
            };
            var result = await _dbHelper.ExecuteQueryAsync<RolePermission>(sql, parameters);
            return result.FirstOrDefault();
        }
    }
}
