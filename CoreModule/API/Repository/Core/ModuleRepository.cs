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
    public class ModuleRepository : IModuleRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public ModuleRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            var query = "SELECT * FROM core.modules";
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
            var query = "SELECT * FROM core.modules WHERE module_id = @ModuleID";
            var parameters = new[] { new NpgsqlParameter("@ModuleID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToModule(dataTable.Rows[0]) : null;
        }

        public async Task<Module> AddAsync(Module module)
        {
            var query = @"
                INSERT INTO core.modules (module_name, is_system_module, is_active)
                VALUES (@ModuleName, @IsSystemModule, @IsActive)
                RETURNING module_id, module_name, is_system_module, is_active;";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModuleName", module.ModuleName),
                new NpgsqlParameter("@IsSystemModule", module.IsSystemModule),
                new NpgsqlParameter("@IsActive", module.IsActive)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToModule(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Module module)
        {
            var query = @"
                UPDATE core.modules
                SET module_name = @ModuleName, is_system_module = @IsSystemModule, is_active = @IsActive
                WHERE module_id = @ModuleID;";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModuleID", module.ModuleID),
                new NpgsqlParameter("@ModuleName", module.ModuleName),
                new NpgsqlParameter("@IsSystemModule", module.IsSystemModule),
                new NpgsqlParameter("@IsActive", module.IsActive)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.modules WHERE module_id = @ModuleID";
            var parameters = new[] { new NpgsqlParameter("@ModuleID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Module MapToModule(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new Module
            {
                ModuleID = Convert.ToInt32(row["module_id"]),
                ModuleName = row["module_name"].ToString(),
                IsSystemModule = Convert.ToBoolean(row["is_system_module"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            };
        }
    }
}