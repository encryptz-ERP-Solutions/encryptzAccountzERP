using Data.Core;
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

        public async Task<Business> GetByIdAsync(Guid id)
        {
            var query = "SELECT * FROM core.Businesses WHERE BusinessID = @BusinessID";
            var parameters = new[] { new SqlParameter("@BusinessID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToBusiness(dataTable.Rows[0]) : null;
        }

        public async Task<Business> AddAsync(Business business)
        {
            var query = @"
                INSERT INTO core.Businesses (BusinessID, BusinessName, BusinessCode, IsActive, Gstin, TanNumber, AddressLine1, AddressLine2, City, StateID, PinCode, CountryID, CreatedByUserID, CreatedAtUTC, UpdatedByUserID, UpdatedAtUTC)
                OUTPUT INSERTED.*
                VALUES (@BusinessID, @BusinessName, @BusinessCode, @IsActive, @Gstin, @TanNumber, @AddressLine1, @AddressLine2, @City, @StateID, @PinCode, @CountryID, @CreatedByUserID, @CreatedAtUTC, @UpdatedByUserID, @UpdatedAtUTC);";

            var parameters = new[]
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
                new SqlParameter("@CreatedAtUTC", DateTime.UtcNow),
                new SqlParameter("@UpdatedByUserID", business.UpdatedByUserID),
                new SqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToBusiness(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Business business)
        {
            var query = @"
                UPDATE core.Businesses
                SET BusinessName = @BusinessName, IsActive = @IsActive, Gstin = @Gstin, TanNumber = @TanNumber,
                    AddressLine1 = @AddressLine1, AddressLine2 = @AddressLine2, City = @City, StateID = @StateID,
                    PinCode = @PinCode, CountryID = @CountryID, UpdatedByUserID = @UpdatedByUserID, UpdatedAtUTC = @UpdatedAtUTC
                WHERE BusinessID = @BusinessID;";

            var parameters = new[]
            {
                new SqlParameter("@BusinessID", business.BusinessID),
                new SqlParameter("@BusinessName", business.BusinessName),
                new SqlParameter("@IsActive", business.IsActive),
                new SqlParameter("@Gstin", (object)business.Gstin ?? DBNull.Value),
                new SqlParameter("@TanNumber", (object)business.TanNumber ?? DBNull.Value),
                new SqlParameter("@AddressLine1", (object)business.AddressLine1 ?? DBNull.Value),
                new SqlParameter("@AddressLine2", (object)business.AddressLine2 ?? DBNull.Value),
                new SqlParameter("@City", (object)business.City ?? DBNull.Value),
                new SqlParameter("@StateID", (object)business.StateID ?? DBNull.Value),
                new SqlParameter("@PinCode", (object)business.PinCode ?? DBNull.Value),
                new SqlParameter("@CountryID", (object)business.CountryID ?? DBNull.Value),
                new SqlParameter("@UpdatedByUserID", business.UpdatedByUserID),
                new SqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var query = "DELETE FROM core.Businesses WHERE BusinessID = @BusinessID";
            var parameters = new[] { new SqlParameter("@BusinessID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Business MapToBusiness(DataRow row)
        {
            return new Business
            {
                BusinessID = (Guid)row["BusinessID"],
                BusinessName = row["BusinessName"].ToString(),
                BusinessCode = row["BusinessCode"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                Gstin = row["Gstin"] == DBNull.Value ? null : row["Gstin"].ToString(),
                TanNumber = row["TanNumber"] == DBNull.Value ? null : row["TanNumber"].ToString(),
                AddressLine1 = row["AddressLine1"] == DBNull.Value ? null : row["AddressLine1"].ToString(),
                AddressLine2 = row["AddressLine2"] == DBNull.Value ? null : row["AddressLine2"].ToString(),
                City = row["City"] == DBNull.Value ? null : row["City"].ToString(),
                StateID = row["StateID"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["StateID"]),
                PinCode = row["PinCode"] == DBNull.Value ? null : row["PinCode"].ToString(),
                CountryID = row["CountryID"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["CountryID"]),
                CreatedByUserID = (Guid)row["CreatedByUserID"],
                CreatedAtUTC = Convert.ToDateTime(row["CreatedAtUTC"]),
                UpdatedByUserID = (Guid)row["UpdatedByUserID"],
                UpdatedAtUTC = Convert.ToDateTime(row["UpdatedAtUTC"])
            };
        }
    }
}