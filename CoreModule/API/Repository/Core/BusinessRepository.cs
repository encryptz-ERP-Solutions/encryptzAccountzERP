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
    public class BusinessRepository : IBusinessRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;
        private const string BaseBusinessSelectQuery = "SELECT BusinessID, BusinessName, BusinessCode, IsActive, Gstin, TanNumber, AddressLine1, AddressLine2, City, StateID, PinCode, CountryID, CreatedByUserID, CreatedAtUTC, UpdatedByUserID, UpdatedAtUTC FROM core.Businesses";

        public BusinessRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Business>> GetAllAsync()
        {
            var dataTable = await _sqlHelper.ExecuteQueryAsync(BaseBusinessSelectQuery, null);
            var businesses = new List<Business>();
            foreach (DataRow row in dataTable.Rows)
            {
                businesses.Add(MapDataRowToBusiness(row));
            }
            return businesses;
        }

        public async Task<Business?> GetByIdAsync(Guid id)
        {
            var query = $"{BaseBusinessSelectQuery} WHERE BusinessID = @BusinessID";
            var parameters = new[] { new SqlParameter("@BusinessID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToBusiness(dataTable.Rows[0]);
        }

        public async Task<Business> AddAsync(Business business)
        {
            var query = @"
                INSERT INTO core.Businesses (BusinessID, BusinessName, BusinessCode, IsActive, Gstin, TanNumber, AddressLine1, AddressLine2, City, StateID, PinCode, CountryID, CreatedByUserID, CreatedAtUTC, UpdatedByUserID, UpdatedAtUTC)
                VALUES (@BusinessID, @BusinessName, @BusinessCode, @IsActive, @Gstin, @TanNumber, @AddressLine1, @AddressLine2, @City, @StateID, @PinCode, @CountryID, @CreatedByUserID, @CreatedAtUTC, @UpdatedByUserID, @UpdatedAtUTC);

                SELECT * FROM core.Businesses WHERE BusinessID = @BusinessID;";

            var parameters = GetSqlParameters(business);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to add business, SELECT query returned no results.");
            }

            return MapDataRowToBusiness(dataTable.Rows[0]);
        }

        public async Task<Business> UpdateAsync(Business business)
        {
            var query = @"
                UPDATE core.Businesses SET
                    BusinessName = @BusinessName,
                    BusinessCode = @BusinessCode,
                    IsActive = @IsActive,
                    Gstin = @Gstin,
                    TanNumber = @TanNumber,
                    AddressLine1 = @AddressLine1,
                    AddressLine2 = @AddressLine2,
                    City = @City,
                    StateID = @StateID,
                    PinCode = @PinCode,
                    CountryID = @CountryID,
                    UpdatedByUserID = @UpdatedByUserID,
                    UpdatedAtUTC = @UpdatedAtUTC
                WHERE BusinessID = @BusinessID;

                SELECT * FROM core.Businesses WHERE BusinessID = @BusinessID;";

            var parameters = GetSqlParameters(business);
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                throw new DataException("Failed to update business, business may not exist.");
            }

            return MapDataRowToBusiness(dataTable.Rows[0]);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var query = "DELETE FROM core.Businesses WHERE BusinessID = @BusinessID";
            var parameters = new[] { new SqlParameter("@BusinessID", id) };
            int rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private static Business MapDataRowToBusiness(DataRow row)
        {
            return new Business
            {
                BusinessID = row.Field<Guid>("BusinessID"),
                BusinessName = row.Field<string>("BusinessName") ?? string.Empty,
                BusinessCode = row.Field<string>("BusinessCode") ?? string.Empty,
                IsActive = row.Field<bool>("IsActive"),
                Gstin = row.Field<string?>("Gstin"),
                TanNumber = row.Field<string?>("TanNumber"),
                AddressLine1 = row.Field<string?>("AddressLine1"),
                AddressLine2 = row.Field<string?>("AddressLine2"),
                City = row.Field<string?>("City"),
                StateID = row.Field<int?>("StateID"),
                PinCode = row.Field<string?>("PinCode"),
                CountryID = row.Field<int?>("CountryID"),
                CreatedByUserID = row.Field<Guid>("CreatedByUserID"),
                CreatedAtUTC = row.Field<DateTime>("CreatedAtUTC"),
                UpdatedByUserID = row.Field<Guid>("UpdatedByUserID"),
                UpdatedAtUTC = row.Field<DateTime>("UpdatedAtUTC")
            };
        }

        private static SqlParameter[] GetSqlParameters(Business business)
        {
            return new[]
            {
                new SqlParameter("@BusinessID", business.BusinessID),
                new SqlParameter("@BusinessName", business.BusinessName),
                new SqlParameter("@BusinessCode", business.BusinessCode),
                new SqlParameter("@IsActive", business.IsActive),
                new SqlParameter("@Gstin", (object)business.Gstin ?? DBNull.Value),
                new SqlParameter("@TanNumber", (object)business.TanNumber ?? DBNull.Value),
                new SqlParameter("@AddressLine1", (object)business.AddressLine1 ?? DBNull.Value),
                new SqlParameter("@AddressLine2", (object)business.AddressLine2 ?? DBNull.Value),
                new SqlParameter("@City", (object)business.City ?? DBNull.Value),
                new SqlParameter("@StateID", (object)business.StateID ?? DBNull.Value),
                new SqlParameter("@PinCode", (object)business.PinCode ?? DBNull.Value),
                new SqlParameter("@CountryID", (object)business.CountryID ?? DBNull.Value),
                new SqlParameter("@CreatedByUserID", business.CreatedByUserID),
                new SqlParameter("@CreatedAtUTC", business.CreatedAtUTC),
                new SqlParameter("@UpdatedByUserID", business.UpdatedByUserID),
                new SqlParameter("@UpdatedAtUTC", business.UpdatedAtUTC)
            };
        }
    }
}