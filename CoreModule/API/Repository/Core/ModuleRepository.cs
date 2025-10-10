using Entities.Core;
using Infrastructure;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repository.Core
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public ModuleRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Modules";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var modules = new List<Module>();

            foreach (DataRow row in dataTable.Rows)
            {
                modules.Add(MapToModule(row));
            }

            return modules;
        }

        public async Task<Module> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.Modules WHERE ModuleID = @ModuleID";
            var parameters = new[] { new SqlParameter("@ModuleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToModule(dataTable.Rows[0]) : null;
        }

        public async Task<Module> AddAsync(Module module)
        {
            var query = @"
                INSERT INTO core.Modules (ModuleName, IsSystemModule, IsActive)
                OUTPUT INSERTED.*
                VALUES (@ModuleName, @IsSystemModule, @IsActive);";

            var parameters = new[]
            {
                new SqlParameter("@ModuleName", module.ModuleName),
                new SqlParameter("@IsSystemModule", module.IsSystemModule),
                new SqlParameter("@IsActive", module.IsActive)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToModule(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Module module)
        {
            var query = @"
                UPDATE core.Modules
                SET ModuleName = @ModuleName, IsSystemModule = @IsSystemModule, IsActive = @IsActive
                WHERE ModuleID = @ModuleID;";

            var parameters = new[]
            {
                new SqlParameter("@ModuleID", module.ModuleID),
                new SqlParameter("@ModuleName", module.ModuleName),
                new SqlParameter("@IsSystemModule", module.IsSystemModule),
                new SqlParameter("@IsActive", module.IsActive)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Modules WHERE ModuleID = @ModuleID";
            var parameters = new[] { new SqlParameter("@ModuleID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Module MapToModule(DataRow row)
        {
            return new Module
            {
                ModuleID = Convert.ToInt32(row["ModuleID"]),
                ModuleName = row["ModuleName"].ToString(),
                IsSystemModule = Convert.ToBoolean(row["IsSystemModule"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}