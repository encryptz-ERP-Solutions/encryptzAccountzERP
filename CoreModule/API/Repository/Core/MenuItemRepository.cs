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
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseMenuItemSelectQuery = "SELECT MenuItemID, ModuleID, ParentMenuItemID, MenuText, MenuURL, IconClass, DisplayOrder, IsActive FROM core.MenuItems";

        public MenuItemRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            var dataTable = await _sqlHelper.ExecuteQueryAsync(BaseMenuItemSelectQuery, null);
            var menuItems = new List<MenuItem>();
            foreach (DataRow row in dataTable.Rows)
            {
                menuItems.Add(MapDataRowToMenuItem(row));
            }
            return menuItems;
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            var query = $"{BaseMenuItemSelectQuery} WHERE MenuItemID = @MenuItemID";
            var parameters = new[] { new SqlParameter("@MenuItemID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToMenuItem(dataTable.Rows[0]);
        }

        public async Task<MenuItem> AddAsync(MenuItem menuItem)
        {
            var query = @"
                INSERT INTO core.MenuItems (ModuleID, ParentMenuItemID, MenuText, MenuURL, IconClass, DisplayOrder, IsActive)
                VALUES (@ModuleID, @ParentMenuItemID, @MenuText, @MenuURL, @IconClass, @DisplayOrder, @IsActive);

                SELECT * FROM core.MenuItems WHERE MenuItemID = SCOPE_IDENTITY();";

            var parameters = GetSqlParameters(menuItem);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to add menu item, SELECT query returned no results.");
            }

            return MapDataRowToMenuItem(dataTable.Rows[0]);
        }

        public async Task<MenuItem> UpdateAsync(MenuItem menuItem)
        {
            var query = @"
                UPDATE core.MenuItems SET
                    ModuleID = @ModuleID,
                    ParentMenuItemID = @ParentMenuItemID,
                    MenuText = @MenuText,
                    MenuURL = @MenuURL,
                    IconClass = @IconClass,
                    DisplayOrder = @DisplayOrder,
                    IsActive = @IsActive
                WHERE MenuItemID = @MenuItemID;

                SELECT * FROM core.MenuItems WHERE MenuItemID = @MenuItemID;";

            var parameters = GetSqlParameters(menuItem);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to update menu item, menu item may not exist.");
            }

            return MapDataRowToMenuItem(dataTable.Rows[0]);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM core.MenuItems WHERE MenuItemID = @MenuItemID";
            var parameters = new[] { new SqlParameter("@MenuItemID", id) };
            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private static MenuItem MapDataRowToMenuItem(DataRow row)
        {
            return new MenuItem
            {
                MenuItemID = row.Field<int>("MenuItemID"),
                ModuleID = row.Field<int>("ModuleID"),
                ParentMenuItemID = row.Field<int?>("ParentMenuItemID"),
                MenuText = row.Field<string>("MenuText") ?? string.Empty,
                MenuURL = row.Field<string?>("MenuURL"),
                IconClass = row.Field<string?>("IconClass"),
                DisplayOrder = row.Field<int>("DisplayOrder"),
                IsActive = row.Field<bool>("IsActive")
            };
        }

        private static SqlParameter[] GetSqlParameters(MenuItem menuItem)
        {
            return new[]
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
        }
    }
}