using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Npgsql;

namespace Repository.Accounts
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public VoucherRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<Voucher> GetByIdAsync(Guid voucherId)
        {
            var query = "SELECT * FROM vouchers WHERE voucher_id = @VoucherID";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var voucher = MapDataRowToVoucher(dataTable.Rows[0]);
            voucher.VoucherLines = (await GetVoucherLinesAsync(voucherId)).ToList();
            return voucher;
        }

        public async Task<IEnumerable<Voucher>> GetAllByBusinessIdAsync(
            Guid businessId,
            string? status = null,
            string? voucherType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = "SELECT * FROM vouchers WHERE business_id = @BusinessID";
            var paramsList = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@BusinessID", businessId)
            };

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND status = @Status";
                paramsList.Add(new NpgsqlParameter("@Status", status));
            }

            if (!string.IsNullOrEmpty(voucherType))
            {
                query += " AND voucher_type = @VoucherType";
                paramsList.Add(new NpgsqlParameter("@VoucherType", voucherType));
            }

            if (startDate.HasValue)
            {
                query += " AND voucher_date >= @StartDate";
                paramsList.Add(new NpgsqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                query += " AND voucher_date <= @EndDate";
                paramsList.Add(new NpgsqlParameter("@EndDate", endDate.Value));
            }

            query += " ORDER BY voucher_date DESC, created_at DESC";

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, paramsList.ToArray());
            var vouchers = new List<Voucher>();

            foreach (DataRow row in dataTable.Rows)
            {
                var voucher = MapDataRowToVoucher(row);
                voucher.VoucherLines = (await GetVoucherLinesAsync(voucher.VoucherID)).ToList();
                vouchers.Add(voucher);
            }

            return vouchers;
        }

        public async Task<Voucher> CreateAsync(Voucher voucher)
        {
            var query = @"
                INSERT INTO vouchers (
                    voucher_id, business_id, voucher_number, voucher_type, voucher_date,
                    reference_number, reference_date, party_account_id, party_name, party_gstin,
                    place_of_supply, total_amount, tax_amount, discount_amount, round_off_amount,
                    net_amount, currency, exchange_rate, status, narration,
                    is_reverse_charge, is_bill_of_supply, due_date, payment_terms,
                    warehouse_id, cost_center, project, created_at, updated_at, created_by, updated_by
                )
                VALUES (
                    @VoucherID, @BusinessID, @VoucherNumber, @VoucherType, @VoucherDate,
                    @ReferenceNumber, @ReferenceDate, @PartyAccountID, @PartyName, @PartyGstin,
                    @PlaceOfSupply, @TotalAmount, @TaxAmount, @DiscountAmount, @RoundOffAmount,
                    @NetAmount, @Currency, @ExchangeRate, @Status, @Narration,
                    @IsReverseCharge, @IsBillOfSupply, @DueDate, @PaymentTerms,
                    @WarehouseID, @CostCenter, @Project, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy
                )";

            var parameters = BuildVoucherParameters(voucher);
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);

            // Insert voucher lines
            foreach (var line in voucher.VoucherLines)
            {
                line.VoucherID = voucher.VoucherID;
                await CreateVoucherLineAsync(line);
            }

            return await GetByIdAsync(voucher.VoucherID);
        }

        public async Task<bool> UpdateAsync(Voucher voucher)
        {
            var query = @"
                UPDATE vouchers
                SET voucher_date = @VoucherDate,
                    reference_number = @ReferenceNumber,
                    reference_date = @ReferenceDate,
                    party_account_id = @PartyAccountID,
                    party_name = @PartyName,
                    party_gstin = @PartyGstin,
                    place_of_supply = @PlaceOfSupply,
                    total_amount = @TotalAmount,
                    tax_amount = @TaxAmount,
                    discount_amount = @DiscountAmount,
                    round_off_amount = @RoundOffAmount,
                    net_amount = @NetAmount,
                    narration = @Narration,
                    is_reverse_charge = @IsReverseCharge,
                    is_bill_of_supply = @IsBillOfSupply,
                    due_date = @DueDate,
                    payment_terms = @PaymentTerms,
                    warehouse_id = @WarehouseID,
                    cost_center = @CostCenter,
                    project = @Project,
                    updated_at = @UpdatedAt,
                    updated_by = @UpdatedBy
                WHERE voucher_id = @VoucherID";

            var parameters = BuildVoucherParameters(voucher);
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);

            // Delete existing lines and insert new ones
            await DeleteVoucherLinesAsync(voucher.VoucherID);
            foreach (var line in voucher.VoucherLines)
            {
                line.VoucherID = voucher.VoucherID;
                await CreateVoucherLineAsync(line);
            }

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid voucherId)
        {
            // Delete lines first due to foreign key constraint
            await DeleteVoucherLinesAsync(voucherId);

            var query = "DELETE FROM vouchers WHERE voucher_id = @VoucherID";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<string> GenerateVoucherNumberAsync(Guid businessId, string voucherType)
        {
            var query = @"
                SELECT COUNT(*) 
                FROM vouchers 
                WHERE business_id = @BusinessID 
                AND voucher_type = @VoucherType 
                AND EXTRACT(YEAR FROM voucher_date) = EXTRACT(YEAR FROM CURRENT_DATE)";

            var parameters = new[]
            {
                new NpgsqlParameter("@BusinessID", businessId),
                new NpgsqlParameter("@VoucherType", voucherType)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var count = Convert.ToInt32(dataTable.Rows[0][0]);
            var year = DateTime.Now.Year.ToString().Substring(2);
            var prefix = GetVoucherTypePrefix(voucherType);

            return $"{prefix}{year}{(count + 1):D5}";
        }

        public async Task<bool> PostVoucherAsync(Guid voucherId, Guid postedBy)
        {
            var query = @"
                UPDATE vouchers
                SET status = 'posted',
                    posted_at = @PostedAt,
                    posted_by = @PostedBy,
                    updated_at = @UpdatedAt
                WHERE voucher_id = @VoucherID 
                AND status = 'draft'";

            var parameters = new[]
            {
                new NpgsqlParameter("@VoucherID", voucherId),
                new NpgsqlParameter("@PostedAt", DateTime.UtcNow),
                new NpgsqlParameter("@PostedBy", postedBy),
                new NpgsqlParameter("@UpdatedAt", DateTime.UtcNow)
            };

            var result = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }

        public async Task<IEnumerable<VoucherLine>> GetVoucherLinesAsync(Guid voucherId)
        {
            var query = "SELECT * FROM voucher_lines WHERE voucher_id = @VoucherID ORDER BY line_number";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            var lines = new List<VoucherLine>();
            foreach (DataRow row in dataTable.Rows)
            {
                lines.Add(MapDataRowToVoucherLine(row));
            }

            return lines;
        }

        private async Task CreateVoucherLineAsync(VoucherLine line)
        {
            var query = @"
                INSERT INTO voucher_lines (
                    line_id, voucher_id, line_number, account_id, item_id, description,
                    quantity, unit_price, discount_percentage, discount_amount, taxable_amount,
                    tax_code_id, tax_rate, tax_amount, cgst_amount, sgst_amount, igst_amount,
                    cess_amount, line_amount, debit_amount, credit_amount, warehouse_id,
                    cost_center, project, created_at, updated_at
                )
                VALUES (
                    @LineID, @VoucherID, @LineNumber, @AccountID, @ItemID, @Description,
                    @Quantity, @UnitPrice, @DiscountPercentage, @DiscountAmount, @TaxableAmount,
                    @TaxCodeID, @TaxRate, @TaxAmount, @CgstAmount, @SgstAmount, @IgstAmount,
                    @CessAmount, @LineAmount, @DebitAmount, @CreditAmount, @WarehouseID,
                    @CostCenter, @Project, @CreatedAt, @UpdatedAt
                )";

            var parameters = BuildVoucherLineParameters(line);
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private async Task DeleteVoucherLinesAsync(Guid voucherId)
        {
            var query = "DELETE FROM voucher_lines WHERE voucher_id = @VoucherID";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private NpgsqlParameter[] BuildVoucherParameters(Voucher voucher)
        {
            return new[]
            {
                new NpgsqlParameter("@VoucherID", voucher.VoucherID),
                new NpgsqlParameter("@BusinessID", voucher.BusinessID),
                new NpgsqlParameter("@VoucherNumber", voucher.VoucherNumber),
                new NpgsqlParameter("@VoucherType", voucher.VoucherType),
                new NpgsqlParameter("@VoucherDate", voucher.VoucherDate),
                new NpgsqlParameter("@ReferenceNumber", (object)voucher.ReferenceNumber ?? DBNull.Value),
                new NpgsqlParameter("@ReferenceDate", (object)voucher.ReferenceDate ?? DBNull.Value),
                new NpgsqlParameter("@PartyAccountID", (object)voucher.PartyAccountID ?? DBNull.Value),
                new NpgsqlParameter("@PartyName", (object)voucher.PartyName ?? DBNull.Value),
                new NpgsqlParameter("@PartyGstin", (object)voucher.PartyGstin ?? DBNull.Value),
                new NpgsqlParameter("@PlaceOfSupply", (object)voucher.PlaceOfSupply ?? DBNull.Value),
                new NpgsqlParameter("@TotalAmount", voucher.TotalAmount),
                new NpgsqlParameter("@TaxAmount", voucher.TaxAmount),
                new NpgsqlParameter("@DiscountAmount", voucher.DiscountAmount),
                new NpgsqlParameter("@RoundOffAmount", voucher.RoundOffAmount),
                new NpgsqlParameter("@NetAmount", voucher.NetAmount),
                new NpgsqlParameter("@Currency", voucher.Currency),
                new NpgsqlParameter("@ExchangeRate", voucher.ExchangeRate),
                new NpgsqlParameter("@Status", voucher.Status),
                new NpgsqlParameter("@Narration", (object)voucher.Narration ?? DBNull.Value),
                new NpgsqlParameter("@IsReverseCharge", voucher.IsReverseCharge),
                new NpgsqlParameter("@IsBillOfSupply", voucher.IsBillOfSupply),
                new NpgsqlParameter("@DueDate", (object)voucher.DueDate ?? DBNull.Value),
                new NpgsqlParameter("@PaymentTerms", (object)voucher.PaymentTerms ?? DBNull.Value),
                new NpgsqlParameter("@WarehouseID", (object)voucher.WarehouseID ?? DBNull.Value),
                new NpgsqlParameter("@CostCenter", (object)voucher.CostCenter ?? DBNull.Value),
                new NpgsqlParameter("@Project", (object)voucher.Project ?? DBNull.Value),
                new NpgsqlParameter("@CreatedAt", voucher.CreatedAt),
                new NpgsqlParameter("@UpdatedAt", voucher.UpdatedAt),
                new NpgsqlParameter("@CreatedBy", (object)voucher.CreatedBy ?? DBNull.Value),
                new NpgsqlParameter("@UpdatedBy", (object)voucher.UpdatedBy ?? DBNull.Value)
            };
        }

        private NpgsqlParameter[] BuildVoucherLineParameters(VoucherLine line)
        {
            return new[]
            {
                new NpgsqlParameter("@LineID", line.LineID),
                new NpgsqlParameter("@VoucherID", line.VoucherID),
                new NpgsqlParameter("@LineNumber", line.LineNumber),
                new NpgsqlParameter("@AccountID", line.AccountID),
                new NpgsqlParameter("@ItemID", (object)line.ItemID ?? DBNull.Value),
                new NpgsqlParameter("@Description", (object)line.Description ?? DBNull.Value),
                new NpgsqlParameter("@Quantity", (object)line.Quantity ?? DBNull.Value),
                new NpgsqlParameter("@UnitPrice", (object)line.UnitPrice ?? DBNull.Value),
                new NpgsqlParameter("@DiscountPercentage", (object)line.DiscountPercentage ?? DBNull.Value),
                new NpgsqlParameter("@DiscountAmount", line.DiscountAmount),
                new NpgsqlParameter("@TaxableAmount", line.TaxableAmount),
                new NpgsqlParameter("@TaxCodeID", (object)line.TaxCodeID ?? DBNull.Value),
                new NpgsqlParameter("@TaxRate", (object)line.TaxRate ?? DBNull.Value),
                new NpgsqlParameter("@TaxAmount", line.TaxAmount),
                new NpgsqlParameter("@CgstAmount", line.CgstAmount),
                new NpgsqlParameter("@SgstAmount", line.SgstAmount),
                new NpgsqlParameter("@IgstAmount", line.IgstAmount),
                new NpgsqlParameter("@CessAmount", line.CessAmount),
                new NpgsqlParameter("@LineAmount", line.LineAmount),
                new NpgsqlParameter("@DebitAmount", line.DebitAmount),
                new NpgsqlParameter("@CreditAmount", line.CreditAmount),
                new NpgsqlParameter("@WarehouseID", (object)line.WarehouseID ?? DBNull.Value),
                new NpgsqlParameter("@CostCenter", (object)line.CostCenter ?? DBNull.Value),
                new NpgsqlParameter("@Project", (object)line.Project ?? DBNull.Value),
                new NpgsqlParameter("@CreatedAt", line.CreatedAt),
                new NpgsqlParameter("@UpdatedAt", line.UpdatedAt)
            };
        }

        private Voucher MapDataRowToVoucher(DataRow row)
        {
            return new Voucher
            {
                VoucherID = (Guid)row["voucher_id"],
                BusinessID = (Guid)row["business_id"],
                VoucherNumber = (string)row["voucher_number"],
                VoucherType = (string)row["voucher_type"],
                VoucherDate = (DateTime)row["voucher_date"],
                ReferenceNumber = row["reference_number"] == DBNull.Value ? null : (string)row["reference_number"],
                ReferenceDate = row["reference_date"] == DBNull.Value ? null : (DateTime?)row["reference_date"],
                PartyAccountID = row["party_account_id"] == DBNull.Value ? null : (Guid?)row["party_account_id"],
                PartyName = row["party_name"] == DBNull.Value ? null : (string)row["party_name"],
                PartyGstin = row["party_gstin"] == DBNull.Value ? null : (string)row["party_gstin"],
                PlaceOfSupply = row["place_of_supply"] == DBNull.Value ? null : (string)row["place_of_supply"],
                TotalAmount = (decimal)row["total_amount"],
                TaxAmount = (decimal)row["tax_amount"],
                DiscountAmount = (decimal)row["discount_amount"],
                RoundOffAmount = (decimal)row["round_off_amount"],
                NetAmount = (decimal)row["net_amount"],
                Currency = (string)row["currency"],
                ExchangeRate = (decimal)row["exchange_rate"],
                Status = (string)row["status"],
                PostedAt = row["posted_at"] == DBNull.Value ? null : (DateTime?)row["posted_at"],
                PostedBy = row["posted_by"] == DBNull.Value ? null : (Guid?)row["posted_by"],
                Narration = row["narration"] == DBNull.Value ? null : (string)row["narration"],
                IsReverseCharge = (bool)row["is_reverse_charge"],
                IsBillOfSupply = (bool)row["is_bill_of_supply"],
                DueDate = row["due_date"] == DBNull.Value ? null : (DateTime?)row["due_date"],
                PaymentTerms = row["payment_terms"] == DBNull.Value ? null : (string)row["payment_terms"],
                WarehouseID = row["warehouse_id"] == DBNull.Value ? null : (Guid?)row["warehouse_id"],
                CostCenter = row["cost_center"] == DBNull.Value ? null : (string)row["cost_center"],
                Project = row["project"] == DBNull.Value ? null : (string)row["project"],
                CreatedAt = (DateTime)row["created_at"],
                UpdatedAt = (DateTime)row["updated_at"],
                CreatedBy = row["created_by"] == DBNull.Value ? null : (Guid?)row["created_by"],
                UpdatedBy = row["updated_by"] == DBNull.Value ? null : (Guid?)row["updated_by"]
            };
        }

        private VoucherLine MapDataRowToVoucherLine(DataRow row)
        {
            return new VoucherLine
            {
                LineID = (Guid)row["line_id"],
                VoucherID = (Guid)row["voucher_id"],
                LineNumber = (int)row["line_number"],
                AccountID = (Guid)row["account_id"],
                ItemID = row["item_id"] == DBNull.Value ? null : (Guid?)row["item_id"],
                Description = row["description"] == DBNull.Value ? null : (string)row["description"],
                Quantity = row["quantity"] == DBNull.Value ? null : (decimal?)row["quantity"],
                UnitPrice = row["unit_price"] == DBNull.Value ? null : (decimal?)row["unit_price"],
                DiscountPercentage = row["discount_percentage"] == DBNull.Value ? null : (decimal?)row["discount_percentage"],
                DiscountAmount = (decimal)row["discount_amount"],
                TaxableAmount = (decimal)row["taxable_amount"],
                TaxCodeID = row["tax_code_id"] == DBNull.Value ? null : (Guid?)row["tax_code_id"],
                TaxRate = row["tax_rate"] == DBNull.Value ? null : (decimal?)row["tax_rate"],
                TaxAmount = (decimal)row["tax_amount"],
                CgstAmount = (decimal)row["cgst_amount"],
                SgstAmount = (decimal)row["sgst_amount"],
                IgstAmount = (decimal)row["igst_amount"],
                CessAmount = (decimal)row["cess_amount"],
                LineAmount = (decimal)row["line_amount"],
                DebitAmount = (decimal)row["debit_amount"],
                CreditAmount = (decimal)row["credit_amount"],
                WarehouseID = row["warehouse_id"] == DBNull.Value ? null : (Guid?)row["warehouse_id"],
                CostCenter = row["cost_center"] == DBNull.Value ? null : (string)row["cost_center"],
                Project = row["project"] == DBNull.Value ? null : (string)row["project"],
                CreatedAt = (DateTime)row["created_at"],
                UpdatedAt = (DateTime)row["updated_at"]
            };
        }

        private string GetVoucherTypePrefix(string voucherType)
        {
            return voucherType switch
            {
                "Sales" => "SAL",
                "Purchase" => "PUR",
                "Payment" => "PAY",
                "Receipt" => "REC",
                "Journal" => "JNL",
                "Contra" => "CON",
                "Debit Note" => "DN",
                "Credit Note" => "CN",
                _ => "VOU"
            };
        }
    }
}
