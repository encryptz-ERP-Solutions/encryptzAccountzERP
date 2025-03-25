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
    public class CompanyRepository :ICompanyRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public CompanyRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            var query = "SELECT * FROM core.CompanyM";
            var dataTable = _sqlHelper.ExecuteQuery(query);
            var Companies = new List<Company>();

            foreach (DataRow row in dataTable.Rows)
            {
                Companies.Add(MapDataRowToCompany(row));
            }

            return await Task.FromResult(Companies);
        }

        public async Task<Company> GetByIdAsync(long id)
        {
            var query = "SELECT * FROM core.CompanyM WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            var dataTable = _sqlHelper.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return await Task.FromResult(MapDataRowToCompany(dataTable.Rows[0]));
        }

        public async Task AddAsync(Company Company)
        {
            var query = @"INSERT INTO core.CompanyM (code, name, isActive, tanNo, panNo, businessTypeID, address, nationID, stateID, districtID, pin, gstin, epf, esi, phoneCountryCode, phoneNo) 
                          VALUES (@Code, @Name, @IsActive, @TanNo, @PanNo, @BusinessTypeId, @Address, @NationId, @StateId, @DistrictId, @Pin, @Gstin, @Epf, @Esi, @PhoneCountryCode, @PhoneNo)";

            var parameters = GetSqlParameters(Company);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Company Company)
        {
            var query = @"UPDATE core.CompanyM SET code = @Code, name = @Name, isActive = @IsActive, tanNo = @TanNo, panNo = @PanNo, 
                          businessTypeID = @BusinessTypeId, address = @Address, nationID = @NationId, stateID = @StateId, districtID = @DistrictId, 
                          pin = @Pin, gstin = @Gstin, epf = @Epf, esi = @Esi, phoneCountryCode = @PhoneCountryCode, phoneNo = @PhoneNo 
                          WHERE id = @Id";

            var parameters = GetSqlParameters(Company);
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(long id)
        {
            var query = "DELETE FROM core.CompanyM WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }

        private static Company MapDataRowToCompany(DataRow row)
        {
            return new Company
            {
                Id = Convert.ToInt64(row["id"]),
                Code = row["code"].ToString(),
                Name = row["name"].ToString(),
                IsActive = Convert.ToBoolean(row["isActive"]),
                TanNo = row["tanNo"].ToString(),
                PanNo = row["panNo"].ToString(),
                BusinessTypeId = row["businessTypeID"] as int?,
                Address = row["address"].ToString(),
                NationId = row["nationID"] as int?,
                StateId = row["stateID"] as int?,
                DistrictId = row["districtID"] as int?,
                Pin = row["pin"].ToString(),
                Gstin = row["gstin"].ToString(),
                Epf = row["epf"].ToString(),
                Esi = row["esi"].ToString(),
                PhoneCountryCode = row["phoneCountryCode"].ToString(),
                PhoneNo = row["phoneNo"].ToString(),
            };
        }

        private static SqlParameter[] GetSqlParameters(Company Company)
        {
            return new[]
            {
                new SqlParameter("@Id", Company.Id),
                new SqlParameter("@Code", Company.Code),
                new SqlParameter("@Name", Company.Name ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", Company.IsActive),
                new SqlParameter("@TanNo", Company.TanNo ?? (object)DBNull.Value),
                new SqlParameter("@PanNo", Company.PanNo ?? (object)DBNull.Value),
                new SqlParameter("@BusinessTypeId", Company.BusinessTypeId ?? (object)DBNull.Value),
                new SqlParameter("@Address", Company.Address ?? (object)DBNull.Value),
                new SqlParameter("@NationId", Company.NationId ?? (object)DBNull.Value),
                new SqlParameter("@StateId", Company.StateId ?? (object)DBNull.Value),
                new SqlParameter("@DistrictId", Company.DistrictId ?? (object)DBNull.Value),
                new SqlParameter("@Pin", Company.Pin ?? (object)DBNull.Value),
                new SqlParameter("@Gstin", Company.Gstin ?? (object)DBNull.Value),
                new SqlParameter("@Epf", Company.Epf ?? (object)DBNull.Value),
                new SqlParameter("@Esi", Company.Esi ?? (object)DBNull.Value),
                new SqlParameter("@PhoneCountryCode", Company.PhoneCountryCode ?? (object)DBNull.Value),
                new SqlParameter("@PhoneNo", Company.PhoneNo ?? (object)DBNull.Value)
            };
        }
    }
}
