using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Core;
using Entities.Core;
using Microsoft.Data.SqlClient;
using Repository.Core.Interface;

namespace Repository.Core
{
    public class ModulesRepository : IModulesRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public ModulesRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Modules>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Modules";
            var dataTable = _sqlHelper.ExecuteQuery(query);
            var Companies = new List<Modules>();

            foreach (DataRow row in dataTable.Rows)
            {
                Companies.Add(MapDataRowToModules(row));
            }

            return await Task.FromResult(Companies);
        }

        public async Task<Modules> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.Modules WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            var dataTable = _sqlHelper.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return await Task.FromResult(MapDataRowToModules(dataTable.Rows[0]));
        }

        public async Task AddAsync(Modules Modules)
        {
            var query = @"Insert Into core.Modules( name, isActive )
                            Values(   @name, @isActive )";
            var parameters = GetSqlParameters(Modules);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Modules Modules)
        {
            var query = @"UPDATE core.Modules SET name = @Name, isActive = @IsActive 
                          WHERE id = @Id";

            var parameters = GetSqlParameters(Modules);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Modules WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        private static SqlParameter[] GetSqlParameters(Modules entity)
        {
            return new[]
            {
                new SqlParameter("@id", entity.id),
                new SqlParameter("@name", entity.name),
                new SqlParameter("@isActive", entity.isActive)
                };
        }


        private static Modules MapDataRowToModules(DataRow row)
        {
            return new Modules
            {
                id = Convert.ToInt32(row["id"]),
                name = Convert.ToString(row["name"]) ?? "",
                isActive = Convert.ToBoolean(row["isActive"])
            };
        }
    }
}
