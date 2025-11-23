using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Npgsql;

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

                var headerQuery = @"INSERT INTO acct.transaction_headers
                                    (transaction_header_id, business_id, transaction_date, reference_number, description, created_by_user_id, created_at_utc)
                                    VALUES (@TransactionHeaderID, @BusinessID, @TransactionDate, @ReferenceNumber, @Description, @CreatedByUserID, @CreatedAtUTC)";
                var headerParams = new[]
                {
                    new NpgsqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                    new NpgsqlParameter("@BusinessID", transactionHeader.BusinessID),
                    new NpgsqlParameter("@TransactionDate", transactionHeader.TransactionDate),
                    new NpgsqlParameter("@ReferenceNumber", (object)transactionHeader.ReferenceNumber ?? DBNull.Value),
                    new NpgsqlParameter("@Description", transactionHeader.Description),
                    new NpgsqlParameter("@CreatedByUserID", transactionHeader.CreatedByUserID),
                    new NpgsqlParameter("@CreatedAtUTC", transactionHeader.CreatedAtUTC)
                };
                await _sqlHelper.ExecuteNonQueryAsync(headerQuery, headerParams, useTransaction: true);

                foreach (var detail in transactionHeader.TransactionDetails)
                {
                    var detailQuery = @"INSERT INTO acct.transaction_details
                                        (transaction_header_id, account_id, debit_amount, credit_amount)
                                        VALUES (@TransactionHeaderID, @AccountID, @DebitAmount, @CreditAmount)";
                    var detailParams = new[]
                    {
                        new NpgsqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                        new NpgsqlParameter("@AccountID", detail.AccountID),
                        new NpgsqlParameter("@DebitAmount", detail.DebitAmount),
                        new NpgsqlParameter("@CreditAmount", detail.CreditAmount)
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
            var query = "DELETE FROM acct.transaction_headers WHERE transaction_header_id = @TransactionHeaderID";
            var parameters = new[] { new NpgsqlParameter("@TransactionHeaderID", transactionHeaderId) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<TransactionHeader> GetTransactionByIdAsync(Guid transactionHeaderId)
        {
            var headerQuery = "SELECT * FROM acct.transaction_headers WHERE transaction_header_id = @TransactionHeaderID";
            var headerParams = new[] { new NpgsqlParameter("@TransactionHeaderID", transactionHeaderId) };
            var headerTable = await _sqlHelper.ExecuteQueryAsync(headerQuery, headerParams);

            if (headerTable.Rows.Count == 0) return null;

            var header = MapDataRowToTransactionHeader(headerTable.Rows[0]);

            var detailQuery = "SELECT * FROM acct.transaction_details WHERE transaction_header_id = @TransactionHeaderID";
            var detailParams = new[] { new NpgsqlParameter("@TransactionHeaderID", transactionHeaderId) };
            var detailTable = await _sqlHelper.ExecuteQueryAsync(detailQuery, detailParams);

            header.TransactionDetails = detailTable.AsEnumerable().Select(MapDataRowToTransactionDetail).ToList();

            return header;
        }

        public async Task<IEnumerable<TransactionHeader>> GetTransactionsByBusinessIdAsync(Guid businessId)
        {
            var headerQuery = "SELECT * FROM acct.transaction_headers WHERE business_id = @BusinessID ORDER BY transaction_date DESC";
            var headerParams = new[] { new NpgsqlParameter("@BusinessID", businessId) };
            var headerTable = await _sqlHelper.ExecuteQueryAsync(headerQuery, headerParams);

            if (headerTable.Rows.Count == 0) return new List<TransactionHeader>();

            var headers = headerTable.AsEnumerable().Select(MapDataRowToTransactionHeader).ToList();
            var headerIds = headers.Select(h => h.TransactionHeaderID).ToList();

            if (!headerIds.Any()) return headers;

            var detailQueryBuilder = new StringBuilder("SELECT * FROM acct.transaction_details WHERE transaction_header_id IN (");
            var sqlParameters = new List<NpgsqlParameter>();
            for (int i = 0; i < headerIds.Count; i++)
            {
                var paramName = $"@HeaderID{i}";
                detailQueryBuilder.Append(paramName);
                if (i < headerIds.Count - 1)
                {
                    detailQueryBuilder.Append(", ");
                }
                sqlParameters.Add(new NpgsqlParameter(paramName, headerIds[i]));
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
            var query = @"UPDATE acct.transaction_headers
                        SET reference_number = @ReferenceNumber, description = @Description
                        WHERE transaction_header_id = @TransactionHeaderID";
            var parameters = new[]
            {
                new NpgsqlParameter("@TransactionHeaderID", transactionHeader.TransactionHeaderID),
                new NpgsqlParameter("@ReferenceNumber", (object)transactionHeader.ReferenceNumber ?? DBNull.Value),
                new NpgsqlParameter("@Description", transactionHeader.Description)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private TransactionHeader MapDataRowToTransactionHeader(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new TransactionHeader
            {
                TransactionHeaderID = (Guid)row["transaction_header_id"],
                BusinessID = (Guid)row["business_id"],
                TransactionDate = (DateTime)row["transaction_date"],
                ReferenceNumber = row["reference_number"] as string,
                Description = (string)row["description"],
                CreatedByUserID = (Guid)row["created_by_user_id"],
                CreatedAtUTC = (DateTime)row["created_at_utc"]
            };
        }

        private TransactionDetail MapDataRowToTransactionDetail(DataRow row)
        {
            // Map from PostgreSQL snake_case to C# PascalCase
            return new TransactionDetail
            {
                TransactionDetailID = (long)row["transaction_detail_id"],
                TransactionHeaderID = (Guid)row["transaction_header_id"],
                AccountID = (Guid)row["account_id"],
                DebitAmount = (decimal)row["debit_amount"],
                CreditAmount = (decimal)row["credit_amount"]
            };
        }
    }
}
