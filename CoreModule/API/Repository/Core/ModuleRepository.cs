using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Data.Core;
using Entities.Core;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseModuleSelectQuery = "SELECT ModuleID, ModuleName, IsSystemModule, IsActive FROM core.Modules";

        public ModuleRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            var dataTable = await _sqlHelper.ExecuteQueryAsync(BaseModuleSelectQuery, null);
            var modules = new List<Module>();
            foreach (DataRow row in dataTable.Rows)
            {
                modules.Add(MapDataRowToModule(row));
            }
            return modules;
        }

        public async Task<Module?> GetByIdAsync(int id)
        {
            var query = $"{BaseModuleSelectQuery} WHERE ModuleID = @ModuleID";
            var parameters = new[] { new SqlParameter("@ModuleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToModule(dataTable.Rows[0]);
        }

        public async Task<Module> AddAsync(Module module)
        {
            var query = @"
                INSERT INTO core.Modules (ModuleName, IsSystemModule, IsActive)
                VALUES (@ModuleName, @IsSystemModule, @IsActive);

                SELECT * FROM core.Modules WHERE ModuleID = SCOPE_IDENTITY();";

            var parameters = GetSqlParameters(module);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to add module, SELECT query returned no results.");
            }

            return MapDataRowToModule(dataTable.Rows[0]);
        }

        public async Task<Module> UpdateAsync(Module module)
        {
            var query = @"
                UPDATE core.Modules SET
                    ModuleName = @ModuleName,
                    IsActive = @IsActive
                WHERE ModuleID = @ModuleID;

                SELECT * FROM core.Modules WHERE ModuleID = @ModuleID;";

            var parameters = GetSqlParameters(module);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to update module, module may not exist.");
            }

            return MapDataRowToModule(dataTable.Rows[0]);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Modules WHERE ModuleID = @ModuleID";
            var parameters = new[] { new SqlParameter("@ModuleID", id) };
            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private static Module MapDataRowToModule(DataRow row)
        {
            return new Module
            {
                ModuleID = row.Field<int>("ModuleID"),
                ModuleName = row.Field<string>("ModuleName") ?? string.Empty,
                IsSystemModule = row.Field<bool>("IsSystemModule"),
                IsActive = row.Field<bool>("IsActive")
            };
        }

        private static SqlParameter[] GetSqlParameters(Module module)
        {
            return new[]
            {
                new SqlParameter("@ModuleID", module.ModuleID),
                new SqlParameter("@ModuleName", module.ModuleName),
                new SqlParameter("@IsSystemModule", module.IsSystemModule),
                new SqlParameter("@IsActive", module.IsActive)
            };
        }
    }
}