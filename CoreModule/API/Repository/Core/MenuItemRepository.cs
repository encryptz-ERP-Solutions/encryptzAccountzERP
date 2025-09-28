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
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public MenuItemRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            var query = "SELECT * FROM core.MenuItems";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var menuItems = new List<MenuItem>();

            foreach (DataRow row in dataTable.Rows)
            {
                menuItems.Add(MapToMenuItem(row));
            }

            return menuItems;
        }

        public async Task<MenuItem> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.MenuItems WHERE MenuItemID = @MenuItemID";
            var parameters = new[] { new SqlParameter("@MenuItemID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToMenuItem(dataTable.Rows[0]) : null;
        }

        public async Task<MenuItem> AddAsync(MenuItem menuItem)
        {
            var query = @"
                INSERT INTO core.MenuItems (ModuleID, ParentMenuItemID, MenuText, MenuURL, IconClass, DisplayOrder, IsActive)
                OUTPUT INSERTED.*
                VALUES (@ModuleID, @ParentMenuItemID, @MenuText, @MenuURL, @IconClass, @DisplayOrder, @IsActive);";

            var parameters = new[]
            {
                new SqlParameter("@ModuleID", menuItem.ModuleID),
                new SqlParameter("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value),
                new SqlParameter("@MenuText", menuItem.MenuText),
                new SqlParameter("@MenuURL", (object)menuItem.MenuURL ?? DBNull.Value),
                new SqlParameter("@IconClass", (object)menuItem.IconClass ?? DBNull.Value),
                new SqlParameter("@DisplayOrder", menuItem.DisplayOrder),
                new SqlParameter("@IsActive", menuItem.IsActive)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToMenuItem(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(MenuItem menuItem)
        {
            var query = @"
                UPDATE core.MenuItems
                SET ModuleID = @ModuleID, ParentMenuItemID = @ParentMenuItemID, MenuText = @MenuText,
                    MenuURL = @MenuURL, IconClass = @IconClass, DisplayOrder = @DisplayOrder, IsActive = @IsActive
                WHERE MenuItemID = @MenuItemID;";

            var parameters = new[]
            {
                new SqlParameter("@MenuItemID", menuItem.MenuItemID),
                new SqlParameter("@ModuleID", menuItem.ModuleID),
                new SqlParameter("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value),
                new SqlParameter("@MenuText", menuItem.MenuText),
                new SqlParameter("@MenuURL", (object)menuItem.MenuURL ?? DBNull.Value),
                new SqlParameter("@IconClass", (object)menuItem.IconClass ?? DBNull.Value),
                new SqlParameter("@DisplayOrder", menuItem.DisplayOrder),
                new SqlParameter("@IsActive", menuItem.IsActive)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.MenuItems WHERE MenuItemID = @MenuItemID";
            var parameters = new[] { new SqlParameter("@MenuItemID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private MenuItem MapToMenuItem(DataRow row)
        {
            return new MenuItem
            {
                MenuItemID = Convert.ToInt32(row["MenuItemID"]),
                ModuleID = Convert.ToInt32(row["ModuleID"]),
                ParentMenuItemID = row["ParentMenuItemID"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["ParentMenuItemID"]),
                MenuText = row["MenuText"].ToString(),
                MenuURL = row["MenuURL"] == DBNull.Value ? null : row["MenuURL"].ToString(),
                IconClass = row["IconClass"] == DBNull.Value ? null : row["IconClass"].ToString(),
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}