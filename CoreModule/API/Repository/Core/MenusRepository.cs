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
    public class MenusRepository : IMenusRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public MenusRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Menus>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Menus";
            var dataTable = _sqlHelper.ExecuteQuery(query);
            var Companies = new List<Menus>();

            foreach (DataRow row in dataTable.Rows)
            {
                Companies.Add(MapDataRowToMenus(row));
            }

            return await Task.FromResult(Companies);
        }

        public async Task<Menus> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.Menus WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            var dataTable = _sqlHelper.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return await Task.FromResult(MapDataRowToMenus(dataTable.Rows[0]));
        }

        public async Task AddAsync(Menus Menus)
        {
            var query = @"Insert Into core.Menus(  name, icon, moduleID, parentMenuId, menuUrl, isActive )
                            Values(  @name, @icon, @moduleID, @parentMenuId, @menuUrl, @isActive )";
            var parameters = GetSqlParameters(Menus);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Menus Menus)
        {
            var query = @"UPDATE core.Menus SET name = @Name, isActive = @IsActive, moduleID= @ModuleID, parentMenuId = @ParentMenuId, menuUrl = @MenuUrl, icon = @Icon
                          WHERE id = @Id";

            var parameters = GetSqlParameters(Menus);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var query = "DELETE FROM core.Menus WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        private static SqlParameter[] GetSqlParameters(Menus entity)
        {
            return new[]
            {
                new SqlParameter("@id", entity.id),
                new SqlParameter("@name", entity.name),
                new SqlParameter("@icon", entity.icon ?? (object)DBNull.Value),
                new SqlParameter("@moduleID", entity.moduleID),
                new SqlParameter("@parentMenuId", entity.parentMenuId),
                new SqlParameter("@menuUrl", entity.menuUrl ?? (object)DBNull.Value),
                new SqlParameter("@isActive", entity.isActive)
                };
        }


        private static Menus MapDataRowToMenus(DataRow row)
        {
            return new Menus
            {
                id = Convert.ToInt32(row["id"]),
                name = Convert.ToString(row["name"]) ?? "",
                icon = row["icon"].ToString(),
                moduleID = Convert.ToInt32(row["moduleID"]),
                parentMenuId = Convert.ToInt32(row["parentMenuId"]),
                menuUrl = row["menuUrl"].ToString(),
                isActive = Convert.ToBoolean(row["isActive"])
            };
        }
    }
}
