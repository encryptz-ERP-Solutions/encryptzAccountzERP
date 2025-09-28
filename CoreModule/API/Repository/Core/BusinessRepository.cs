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
    public class BusinessRepository : IBusinessRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public BusinessRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Business>> GetAllAsync()
        {
            var query = "SELECT * FROM core.Businesses";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var businesses = new List<Business>();

            foreach (DataRow row in dataTable.Rows)
            {
                businesses.Add(MapToBusiness(row));
            }

            return businesses;
        }

        public async Task<Business> GetByIdAsync(long id)
        {
            var query = "SELECT * FROM core.Businesses WHERE BusinessId = @BusinessId";
            var parameters = new[] { new SqlParameter("@BusinessId", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToBusiness(dataTable.Rows[0]) : null;
        }

        public async Task<Business> AddAsync(Business business)
        {
            var query = @"
                INSERT INTO core.Businesses (Name, Address, IsActive, CreatedAtUTC, UpdatedAtUTC)
                OUTPUT INSERTED.*
                VALUES (@Name, @Address, @IsActive, @CreatedAtUTC, @UpdatedAtUTC);";

            var parameters = new[]
            {
                new SqlParameter("@Name", business.Name),
                new SqlParameter("@Address", business.Address),
                new SqlParameter("@IsActive", business.IsActive),
                new SqlParameter("@CreatedAtUTC", DateTime.UtcNow),
                new SqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToBusiness(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Business business)
        {
            var query = @"
                UPDATE core.Businesses
                SET Name = @Name, Address = @Address, IsActive = @IsActive, UpdatedAtUTC = @UpdatedAtUTC
                WHERE BusinessId = @BusinessId;";

            var parameters = new[]
            {
                new SqlParameter("@BusinessId", business.BusinessId),
                new SqlParameter("@Name", business.Name),
                new SqlParameter("@Address", business.Address),
                new SqlParameter("@IsActive", business.IsActive),
                new SqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var query = "DELETE FROM core.Businesses WHERE BusinessId = @BusinessId";
            var parameters = new[] { new SqlParameter("@BusinessId", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Business MapToBusiness(DataRow row)
        {
            return new Business
            {
                BusinessId = Convert.ToInt64(row["BusinessId"]),
                Name = row["Name"].ToString(),
                Address = row["Address"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAtUTC = Convert.ToDateTime(row["CreatedAtUTC"]),
                UpdatedAtUTC = Convert.ToDateTime(row["UpdatedAtUTC"])
            };
        }
    }
}