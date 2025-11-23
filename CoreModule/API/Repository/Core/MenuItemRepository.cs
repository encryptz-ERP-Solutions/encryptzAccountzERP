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
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public MenuItemRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            var query = "SELECT * FROM core.menu_items ORDER BY display_order";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var menuItems = new List<MenuItem>();

            foreach (DataRow row in dataTable.Rows)
            {
                menuItems.Add(MapToMenuItem(row));
            }

            return menuItems;
        }

        public async Task<IEnumerable<MenuItem>> GetByModuleIdAsync(int moduleId)
        {
            var query = @"
                SELECT * 
                FROM core.menu_items 
                WHERE module_id = @ModuleID
                ORDER BY display_order";

            var parameters = new[] { new NpgsqlParameter("@ModuleID", moduleId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var menuItems = new List<MenuItem>();

            foreach (DataRow row in dataTable.Rows)
            {
                menuItems.Add(MapToMenuItem(row));
            }

            return menuItems;
        }

        public async Task<MenuItem> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM core.menu_items WHERE menu_item_id = @MenuItemID";
            var parameters = new[] { new NpgsqlParameter("@MenuItemID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToMenuItem(dataTable.Rows[0]) : null;
        }

        public async Task<MenuItem> AddAsync(MenuItem menuItem)
        {
            var query = @"
                INSERT INTO core.menu_items (module_id, parent_menu_item_id, menu_text, menu_url, icon_class, display_order, is_active)
                VALUES (@ModuleID, @ParentMenuItemID, @MenuText, @MenuURL, @IconClass, @DisplayOrder, @IsActive)
                RETURNING menu_item_id, module_id, parent_menu_item_id, menu_text, menu_url, icon_class, display_order, is_active;";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModuleID", menuItem.ModuleID),
                new NpgsqlParameter("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value),
                new NpgsqlParameter("@MenuText", menuItem.MenuText),
                new NpgsqlParameter("@MenuURL", (object)menuItem.MenuURL ?? DBNull.Value),
                new NpgsqlParameter("@IconClass", (object)menuItem.IconClass ?? DBNull.Value),
                new NpgsqlParameter("@DisplayOrder", menuItem.DisplayOrder),
                new NpgsqlParameter("@IsActive", menuItem.IsActive)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToMenuItem(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(MenuItem menuItem)
        {
            var query = @"
                UPDATE core.menu_items
                SET module_id = @ModuleID, parent_menu_item_id = @ParentMenuItemID, menu_text = @MenuText,
                    menu_url = @MenuURL, icon_class = @IconClass, display_order = @DisplayOrder, is_active = @IsActive
                WHERE menu_item_id = @MenuItemID;";

            var parameters = new[]
            {
                new NpgsqlParameter("@MenuItemID", menuItem.MenuItemID),
                new NpgsqlParameter("@ModuleID", menuItem.ModuleID),
                new NpgsqlParameter("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value),
                new NpgsqlParameter("@MenuText", menuItem.MenuText),
                new NpgsqlParameter("@MenuURL", (object)menuItem.MenuURL ?? DBNull.Value),
                new NpgsqlParameter("@IconClass", (object)menuItem.IconClass ?? DBNull.Value),
                new NpgsqlParameter("@DisplayOrder", menuItem.DisplayOrder),
                new NpgsqlParameter("@IsActive", menuItem.IsActive)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.menu_items WHERE menu_item_id = @MenuItemID";
            var parameters = new[] { new NpgsqlParameter("@MenuItemID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private MenuItem MapToMenuItem(DataRow row)
        {
            return new MenuItem
            {
                MenuItemID = Convert.ToInt32(row["menu_item_id"]),
                ModuleID = Convert.ToInt32(row["module_id"]),
                ParentMenuItemID = row["parent_menu_item_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["parent_menu_item_id"]),
                MenuText = row["menu_text"].ToString(),
                MenuURL = row["menu_url"] == DBNull.Value ? null : row["menu_url"].ToString(),
                IconClass = row["icon_class"] == DBNull.Value ? null : row["icon_class"].ToString(),
                DisplayOrder = Convert.ToInt32(row["display_order"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            };
        }
    }
}