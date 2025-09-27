using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Data.Core;
using Entities.Core;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BasePermissionSelectQuery = "SELECT PermissionID, PermissionKey, Description, ModuleID FROM core.Permissions";

        public PermissionRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            var dataTable = await _sqlHelper.ExecuteQueryAsync(BasePermissionSelectQuery, null);
            var permissions = new List<Permission>();
            foreach (DataRow row in dataTable.Rows)
            {
                permissions.Add(MapDataRowToPermission(row));
            }
            return permissions;
        }

        private static Permission MapDataRowToPermission(DataRow row)
        {
            return new Permission
            {
                PermissionID = row.Field<int>("PermissionID"),
                PermissionKey = row.Field<string>("PermissionKey") ?? string.Empty,
                Description = row.Field<string>("Description") ?? string.Empty,
                ModuleID = row.Field<int>("ModuleID")
            };
        }
    }
}