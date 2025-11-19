
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
    public class BusinessRepository : IBusinessRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public BusinessRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Business>> GetAllAsync()
        {
            var query = "SELECT * FROM core.businesses";
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
            var query = "SELECT * FROM core.businesses WHERE business_id = @BusinessID";
            var parameters = new[] { new NpgsqlParameter("@BusinessID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            return dataTable.Rows.Count > 0 ? MapToBusiness(dataTable.Rows[0]) : null;
        }

        public async Task<Business> AddAsync(Business business)
        {
            var query = @"
                INSERT INTO core.businesses (business_id, business_name, business_code, is_active, gstin, tan_number, address_line1, address_line2, city, state_id, pin_code, country_id, created_by_user_id, created_at_utc, updated_by_user_id, updated_at_utc)
                VALUES (@BusinessID, @BusinessName, @BusinessCode, @IsActive, @Gstin, @TanNumber, @AddressLine1, @AddressLine2, @City, @StateID, @PinCode, @CountryID, @CreatedByUserID, @CreatedAtUTC, @UpdatedByUserID, @UpdatedAtUTC)
                RETURNING business_id, business_name, business_code, is_active, gstin, tan_number, address_line1, address_line2, city, state_id, pin_code, country_id, created_by_user_id, created_at_utc, updated_by_user_id, updated_at_utc;";

            var parameters = new[]
            {
                new NpgsqlParameter("@BusinessID", business.BusinessID),
                new NpgsqlParameter("@BusinessName", business.BusinessName),
                new NpgsqlParameter("@BusinessCode", business.BusinessCode),
                new NpgsqlParameter("@IsActive", business.IsActive),
                new NpgsqlParameter("@Gstin", (object)business.Gstin ?? DBNull.Value),
                new NpgsqlParameter("@TanNumber", (object)business.TanNumber ?? DBNull.Value),
                new NpgsqlParameter("@AddressLine1", (object)business.AddressLine1 ?? DBNull.Value),
                new NpgsqlParameter("@AddressLine2", (object)business.AddressLine2 ?? DBNull.Value),
                new NpgsqlParameter("@City", (object)business.City ?? DBNull.Value),
                new NpgsqlParameter("@StateID", (object)business.StateID ?? DBNull.Value),
                new NpgsqlParameter("@PinCode", (object)business.PinCode ?? DBNull.Value),
                new NpgsqlParameter("@CountryID", (object)business.CountryID ?? DBNull.Value),
                new NpgsqlParameter("@CreatedByUserID", business.CreatedByUserID),
                new NpgsqlParameter("@CreatedAtUTC", DateTime.UtcNow),
                new NpgsqlParameter("@UpdatedByUserID", business.UpdatedByUserID),
                new NpgsqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapToBusiness(dataTable.Rows[0]);
        }

        public async Task<bool> UpdateAsync(Business business)
        {
            var query = @"
                UPDATE core.businesses
                SET business_name = @BusinessName, is_active = @IsActive, gstin = @Gstin, tan_number = @TanNumber,
                    address_line1 = @AddressLine1, address_line2 = @AddressLine2, city = @City, state_id = @StateID,
                    pin_code = @PinCode, country_id = @CountryID, updated_by_user_id = @UpdatedByUserID, updated_at_utc = @UpdatedAtUTC
                WHERE business_id = @BusinessID;";

            var parameters = new[]
            {
                new NpgsqlParameter("@BusinessID", business.BusinessID),
                new NpgsqlParameter("@BusinessName", business.BusinessName),
                new NpgsqlParameter("@IsActive", business.IsActive),
                new NpgsqlParameter("@Gstin", (object)business.Gstin ?? DBNull.Value),
                new NpgsqlParameter("@TanNumber", (object)business.TanNumber ?? DBNull.Value),
                new NpgsqlParameter("@AddressLine1", (object)business.AddressLine1 ?? DBNull.Value),
                new NpgsqlParameter("@AddressLine2", (object)business.AddressLine2 ?? DBNull.Value),
                new NpgsqlParameter("@City", (object)business.City ?? DBNull.Value),
                new NpgsqlParameter("@StateID", (object)business.StateID ?? DBNull.Value),
                new NpgsqlParameter("@PinCode", (object)business.PinCode ?? DBNull.Value),
                new NpgsqlParameter("@CountryID", (object)business.CountryID ?? DBNull.Value),
                new NpgsqlParameter("@UpdatedByUserID", business.UpdatedByUserID),
                new NpgsqlParameter("@UpdatedAtUTC", DateTime.UtcNow)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var query = "DELETE FROM core.businesses WHERE business_id = @BusinessID";
            var parameters = new[] { new NpgsqlParameter("@BusinessID", id) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        private Business MapToBusiness(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new Business
            {
                BusinessID = (Guid)row["business_id"],
                BusinessName = row["business_name"].ToString(),
                BusinessCode = row["business_code"].ToString(),
                IsActive = Convert.ToBoolean(row["is_active"]),
                Gstin = row["gstin"] == DBNull.Value ? null : row["gstin"].ToString(),
                TanNumber = row["tan_number"] == DBNull.Value ? null : row["tan_number"].ToString(),
                AddressLine1 = row["address_line1"] == DBNull.Value ? null : row["address_line1"].ToString(),
                AddressLine2 = row["address_line2"] == DBNull.Value ? null : row["address_line2"].ToString(),
                City = row["city"] == DBNull.Value ? null : row["city"].ToString(),
                StateID = row["state_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["state_id"]),
                PinCode = row["pin_code"] == DBNull.Value ? null : row["pin_code"].ToString(),
                CountryID = row["country_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["country_id"]),
                CreatedByUserID = (Guid)row["created_by_user_id"],
                CreatedAtUTC = Convert.ToDateTime(row["created_at_utc"]),
                UpdatedByUserID = (Guid)row["updated_by_user_id"],
                UpdatedAtUTC = Convert.ToDateTime(row["updated_at_utc"])
            };
        }
    }
}