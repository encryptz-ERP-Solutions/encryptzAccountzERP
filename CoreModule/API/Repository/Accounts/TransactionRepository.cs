using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Microsoft.Data.SqlClient;

namespace Repository.Accounts
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public TransactionRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<TransactionHeader> CreateTransactionAsync(TransactionHeader transactionHeader)
        {
            try
            {
                _sqlHelper.BeginTransaction();

                var headerQuery = @"INSERT INTO Acct.TransactionHeaders
                                    (TransactionHeaderID, BusinessID, TransactionDate, ReferenceNumber, Description, CreatedByUserID, CreatedAtUTC)
                                    VALUES (@TransactionHeaderID, @BusinessID, @TransactionDate, @ReferenceNumber, @Description, @CreatedByUserID, @CreatedAtUTC)";
                var headerParams = new[]
                {
                    new SqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                    new SqlParameter("@BusinessID", transactionHeader.BusinessID),
                    new SqlParameter("@TransactionDate", transactionHeader.TransactionDate),
                    new SqlParameter("@ReferenceNumber", (object)transactionHeader.ReferenceNumber ?? DBNull.Value),
                    new SqlParameter("@Description", transactionHeader.Description),
                    new SqlParameter("@CreatedByUserID", transactionHeader.CreatedByUserID),
                    new SqlParameter("@CreatedAtUTC", transactionHeader.CreatedAtUTC)
                };
                await _sqlHelper.ExecuteNonQueryAsync(headerQuery, headerParams, useTransaction: true);

                foreach (var detail in transactionHeader.TransactionDetails)
                {
                    var detailQuery = @"INSERT INTO Acct.TransactionDetails
                                        (TransactionHeaderID, AccountID, DebitAmount, CreditAmount)
                                        VALUES (@TransactionHeaderID, @AccountID, @DebitAmount, @CreditAmount)";
                    var detailParams = new[]
                    {
                        new SqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                        new SqlParameter("@AccountID", detail.AccountID),
                        new SqlParameter("@DebitAmount", detail.DebitAmount),
                        new SqlParameter("@CreditAmount", detail.CreditAmount)
                    };
                    await _sqlHelper.ExecuteNonQueryAsync(detailQuery, detailParams, useTransaction: true);
                }

                _sqlHelper.CommitTransaction();
                return transactionHeader;
            }
            catch (Exception)
            {
                _sqlHelper.RollbackTransaction();
                throw;
            }
        }

        public async Task DeleteTransactionAsync(Guid transactionHeaderId)
        {
            var query = "DELETE FROM Acct.TransactionHeaders WHERE TransactionHeaderID = @TransactionHeaderID";
            var parameters = new[] { new SqlParameter("@TransactionHeaderID", transactionHeaderId) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<TransactionHeader> GetTransactionByIdAsync(Guid transactionHeaderId)
        {
            var headerQuery = "SELECT * FROM Acct.TransactionHeaders WHERE TransactionHeaderID = @TransactionHeaderID";
            var headerParams = new[] { new SqlParameter("@TransactionHeaderID", transactionHeaderId) };
            var headerTable = await _sqlHelper.ExecuteQueryAsync(headerQuery, headerParams);

            if (headerTable.Rows.Count == 0) return null;

            var header = MapDataRowToTransactionHeader(headerTable.Rows[0]);

            var detailQuery = "SELECT * FROM Acct.TransactionDetails WHERE TransactionHeaderID = @TransactionHeaderID";
            var detailParams = new[] { new SqlParameter("@TransactionHeaderID", transactionHeaderId) };
            var detailTable = await _sqlHelper.ExecuteQueryAsync(detailQuery, detailParams);

            header.TransactionDetails = detailTable.AsEnumerable().Select(MapDataRowToTransactionDetail).ToList();

            return header;
        }

        public async Task<IEnumerable<TransactionHeader>> GetTransactionsByBusinessIdAsync(Guid businessId)
        {
            var headerQuery = "SELECT * FROM Acct.TransactionHeaders WHERE BusinessID = @BusinessID ORDER BY TransactionDate DESC";
            var headerParams = new[] { new SqlParameter("@BusinessID", businessId) };
            var headerTable = await _sqlHelper.ExecuteQueryAsync(headerQuery, headerParams);

            if (headerTable.Rows.Count == 0) return new List<TransactionHeader>();

            var headers = headerTable.AsEnumerable().Select(MapDataRowToTransactionHeader).ToList();
            var headerIds = headers.Select(h => h.TransactionHeaderID).ToList();

            if (!headerIds.Any()) return headers;

            var detailQueryBuilder = new StringBuilder("SELECT * FROM Acct.TransactionDetails WHERE TransactionHeaderID IN (");
            var sqlParameters = new List<SqlParameter>();
            for (int i = 0; i < headerIds.Count; i++)
            {
                var paramName = $"@HeaderID{i}";
                detailQueryBuilder.Append(paramName);
                if (i < headerIds.Count - 1)
                {
                    detailQueryBuilder.Append(", ");
                }
                sqlParameters.Add(new SqlParameter(paramName, headerIds[i]));
            }
            detailQueryBuilder.Append(")");

            var detailTable = await _sqlHelper.ExecuteQueryAsync(detailQueryBuilder.ToString(), sqlParameters.ToArray());

            var detailsByHeaderId = detailTable.AsEnumerable()
                .Select(MapDataRowToTransactionDetail)
                .GroupBy(d => d.TransactionHeaderID)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var header in headers)
            {
                if (detailsByHeaderId.TryGetValue(header.TransactionHeaderID, out var details))
                {
                    header.TransactionDetails = details;
                }
                else
                {
                    header.TransactionDetails = new List<TransactionDetail>();
                }
            }

            return headers;
        }

        public async Task UpdateTransactionHeaderAsync(TransactionHeader transactionHeader)
        {
            var query = @"UPDATE Acct.TransactionHeaders
                        SET ReferenceNumber = @ReferenceNumber, Description = @Description
                        WHERE TransactionHeaderID = @TransactionHeaderID";
            var parameters = new[]
            {
                new SqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                new SqlParameter("@ReferenceNumber", (object)transactionHeader.ReferenceNumber ?? DBNull.Value),
                new SqlParameter("@Description", transactionHeader.Description)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private TransactionHeader MapDataRowToTransactionHeader(DataRow row)
        {
            return new TransactionHeader
            {
                TransactionHeaderID = (Guid)row["TransactionHeaderID"],
                BusinessID = (Guid)row["BusinessID"],
                TransactionDate = (DateTime)row["TransactionDate"],
                ReferenceNumber = row["ReferenceNumber"] as string,
                Description = (string)row["Description"],
                CreatedByUserID = (Guid)row["CreatedByUserID"],
                CreatedAtUTC = (DateTime)row["CreatedAtUTC"]
            };
        }

        private TransactionDetail MapDataRowToTransactionDetail(DataRow row)
        {
            return new TransactionDetail
            {
                TransactionDetailID = (long)row["TransactionDetailID"],
                TransactionHeaderID = (Guid)row["TransactionHeaderID"],
                AccountID = (Guid)row["AccountID"],
                DebitAmount = (decimal)row["DebitAmount"],
                CreditAmount = (decimal)row["CreditAmount"]
            };
        }
    }
}
