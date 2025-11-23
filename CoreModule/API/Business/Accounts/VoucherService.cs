using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;
using Repository.Accounts;

namespace BusinessLogic.Accounts
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly ILedgerService _ledgerService;
        private readonly IMapper _mapper;

        public VoucherService(
            IVoucherRepository voucherRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            ILedgerService ledgerService,
            IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _chartOfAccountRepository = chartOfAccountRepository;
            _ledgerService = ledgerService;
            _mapper = mapper;
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(Guid voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            return _mapper.Map<VoucherDto>(voucher);
        }

        public async Task<IEnumerable<VoucherDto>> GetVouchersByBusinessIdAsync(
            Guid businessId,
            string? status = null,
            string? voucherType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var vouchers = await _voucherRepository.GetAllByBusinessIdAsync(businessId, status, voucherType, startDate, endDate);
            return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createDto, Guid createdBy)
        {
            // Validate voucher data
            await ValidateVoucherAsync(createDto);

            // Generate voucher number
            var voucherNumber = await _voucherRepository.GenerateVoucherNumberAsync(createDto.BusinessID, createDto.VoucherType);

            // Create voucher entity
            var voucher = new Voucher
            {
                VoucherID = Guid.NewGuid(),
                BusinessID = createDto.BusinessID,
                VoucherNumber = voucherNumber,
                VoucherType = createDto.VoucherType,
                VoucherDate = createDto.VoucherDate,
                ReferenceNumber = createDto.ReferenceNumber,
                ReferenceDate = createDto.ReferenceDate,
                PartyAccountID = createDto.PartyAccountID,
                PartyName = createDto.PartyName,
                PartyGstin = createDto.PartyGstin,
                PlaceOfSupply = createDto.PlaceOfSupply,
                Narration = createDto.Narration,
                IsReverseCharge = createDto.IsReverseCharge,
                IsBillOfSupply = createDto.IsBillOfSupply,
                DueDate = createDto.DueDate,
                PaymentTerms = createDto.PaymentTerms,
                WarehouseID = createDto.WarehouseID,
                CostCenter = createDto.CostCenter,
                Project = createDto.Project,
                Status = "draft",
                Currency = "INR",
                ExchangeRate = 1.000000m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            // Add voucher lines
            int lineNumber = 1;
            foreach (var lineDto in createDto.Lines)
            {
                var line = new VoucherLine
                {
                    LineID = Guid.NewGuid(),
                    VoucherID = voucher.VoucherID,
                    LineNumber = lineNumber++,
                    AccountID = lineDto.AccountID,
                    ItemID = lineDto.ItemID,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercentage = lineDto.DiscountPercentage,
                    DiscountAmount = lineDto.DiscountAmount,
                    TaxableAmount = lineDto.TaxableAmount,
                    TaxCodeID = lineDto.TaxCodeID,
                    TaxRate = lineDto.TaxRate,
                    TaxAmount = lineDto.TaxAmount,
                    CgstAmount = lineDto.CgstAmount,
                    SgstAmount = lineDto.SgstAmount,
                    IgstAmount = lineDto.IgstAmount,
                    CessAmount = lineDto.CessAmount,
                    LineAmount = lineDto.LineAmount,
                    DebitAmount = lineDto.DebitAmount,
                    CreditAmount = lineDto.CreditAmount,
                    WarehouseID = lineDto.WarehouseID,
                    CostCenter = lineDto.CostCenter,
                    Project = lineDto.Project,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                voucher.VoucherLines.Add(line);
            }

            // Calculate totals
            CalculateVoucherTotals(voucher);

            // Create voucher
            var createdVoucher = await _voucherRepository.CreateAsync(voucher);
            return _mapper.Map<VoucherDto>(createdVoucher);
        }

        public async Task<VoucherDto> UpdateVoucherAsync(Guid voucherId, UpdateVoucherDto updateDto, Guid updatedBy)
        {
            // Get existing voucher
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            if (voucher == null)
                throw new ArgumentException("Voucher not found");

            // Only draft vouchers can be updated
            if (voucher.Status != "draft")
                throw new InvalidOperationException("Only draft vouchers can be updated");

            // Validate voucher data
            await ValidateVoucherAsync(updateDto);

            // Update voucher properties
            voucher.VoucherDate = updateDto.VoucherDate;
            voucher.ReferenceNumber = updateDto.ReferenceNumber;
            voucher.ReferenceDate = updateDto.ReferenceDate;
            voucher.PartyAccountID = updateDto.PartyAccountID;
            voucher.PartyName = updateDto.PartyName;
            voucher.PartyGstin = updateDto.PartyGstin;
            voucher.PlaceOfSupply = updateDto.PlaceOfSupply;
            voucher.Narration = updateDto.Narration;
            voucher.IsReverseCharge = updateDto.IsReverseCharge;
            voucher.IsBillOfSupply = updateDto.IsBillOfSupply;
            voucher.DueDate = updateDto.DueDate;
            voucher.PaymentTerms = updateDto.PaymentTerms;
            voucher.WarehouseID = updateDto.WarehouseID;
            voucher.CostCenter = updateDto.CostCenter;
            voucher.Project = updateDto.Project;
            voucher.UpdatedAt = DateTime.UtcNow;
            voucher.UpdatedBy = updatedBy;

            // Update voucher lines
            voucher.VoucherLines.Clear();
            int lineNumber = 1;
            foreach (var lineDto in updateDto.Lines)
            {
                var line = new VoucherLine
                {
                    LineID = Guid.NewGuid(),
                    VoucherID = voucher.VoucherID,
                    LineNumber = lineNumber++,
                    AccountID = lineDto.AccountID,
                    ItemID = lineDto.ItemID,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercentage = lineDto.DiscountPercentage,
                    DiscountAmount = lineDto.DiscountAmount,
                    TaxableAmount = lineDto.TaxableAmount,
                    TaxCodeID = lineDto.TaxCodeID,
                    TaxRate = lineDto.TaxRate,
                    TaxAmount = lineDto.TaxAmount,
                    CgstAmount = lineDto.CgstAmount,
                    SgstAmount = lineDto.SgstAmount,
                    IgstAmount = lineDto.IgstAmount,
                    CessAmount = lineDto.CessAmount,
                    LineAmount = lineDto.LineAmount,
                    DebitAmount = lineDto.DebitAmount,
                    CreditAmount = lineDto.CreditAmount,
                    WarehouseID = lineDto.WarehouseID,
                    CostCenter = lineDto.CostCenter,
                    Project = lineDto.Project,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                voucher.VoucherLines.Add(line);
            }

            // Calculate totals
            CalculateVoucherTotals(voucher);

            // Update voucher
            await _voucherRepository.UpdateAsync(voucher);
            var updatedVoucher = await _voucherRepository.GetByIdAsync(voucherId);
            return _mapper.Map<VoucherDto>(updatedVoucher);
        }

        public async Task<bool> DeleteVoucherAsync(Guid voucherId)
        {
            // Get existing voucher
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            if (voucher == null)
                return false;

            // Only draft vouchers can be deleted
            if (voucher.Status != "draft")
                throw new InvalidOperationException("Only draft vouchers can be deleted");

            return await _voucherRepository.DeleteAsync(voucherId);
        }

        public async Task<PostVoucherResponseDto> PostVoucherAsync(Guid voucherId, Guid postedBy)
        {
            // Get existing voucher
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            if (voucher == null)
                throw new ArgumentException("Voucher not found");

            // Only draft vouchers can be posted
            if (voucher.Status != "draft")
                throw new InvalidOperationException("Only draft vouchers can be posted");

            // Validate voucher before posting
            ValidateVoucherForPosting(voucher);

            // Post voucher (update status to 'posted')
            var success = await _voucherRepository.PostVoucherAsync(voucherId, postedBy);

            if (success)
            {
                // Generate ledger entries from the posted voucher
                var ledgerResult = await _ledgerService.PostVoucherToLedgerAsync(voucherId, postedBy);
                
                if (!ledgerResult.Success)
                {
                    // If ledger posting fails, return error but voucher remains in posted status
                    // Admin can review and repost or fix the issue
                    return new PostVoucherResponseDto
                    {
                        Success = false,
                        Message = $"Voucher posted but ledger generation failed: {ledgerResult.Message}",
                        VoucherID = voucherId,
                        PostedAt = DateTime.UtcNow
                    };
                }

                return new PostVoucherResponseDto
                {
                    Success = true,
                    Message = $"Voucher posted successfully. {ledgerResult.EntriesCreated} ledger entries created.",
                    VoucherID = voucherId,
                    PostedAt = DateTime.UtcNow
                };
            }

            return new PostVoucherResponseDto
            {
                Success = false,
                Message = "Failed to post voucher",
                VoucherID = voucherId
            };
        }

        private async Task ValidateVoucherAsync(CreateVoucherDto createDto)
        {
            // Validate voucher type
            var validVoucherTypes = new[] { "Sales", "Purchase", "Payment", "Receipt", "Journal", "Contra", "Debit Note", "Credit Note" };
            if (!validVoucherTypes.Contains(createDto.VoucherType))
                throw new ArgumentException($"Invalid voucher type. Valid types are: {string.Join(", ", validVoucherTypes)}");

            // Validate lines
            if (createDto.Lines == null || !createDto.Lines.Any())
                throw new ArgumentException("At least one voucher line is required");

            // Validate that all accounts exist
            foreach (var line in createDto.Lines)
            {
                var account = await _chartOfAccountRepository.GetChartOfAccountByIdAsync(line.AccountID);
                if (account == null)
                    throw new ArgumentException($"Account with ID {line.AccountID} not found");
                
                if (!account.IsActive)
                    throw new ArgumentException($"Account {account.AccountName} is not active");
            }

            // Validate amounts match
            var totalDebit = createDto.Lines.Sum(l => l.DebitAmount);
            var totalCredit = createDto.Lines.Sum(l => l.CreditAmount);

            if (createDto.VoucherType == "Journal" && totalDebit != totalCredit)
                throw new ArgumentException("Total debit and credit amounts must match for Journal vouchers");
        }

        private async Task ValidateVoucherAsync(UpdateVoucherDto updateDto)
        {
            // Validate lines
            if (updateDto.Lines == null || !updateDto.Lines.Any())
                throw new ArgumentException("At least one voucher line is required");

            // Validate that all accounts exist
            foreach (var line in updateDto.Lines)
            {
                var account = await _chartOfAccountRepository.GetChartOfAccountByIdAsync(line.AccountID);
                if (account == null)
                    throw new ArgumentException($"Account with ID {line.AccountID} not found");
                
                if (!account.IsActive)
                    throw new ArgumentException($"Account {account.AccountName} is not active");
            }
        }

        private void ValidateVoucherForPosting(Voucher voucher)
        {
            // Validate voucher has lines
            if (voucher.VoucherLines == null || !voucher.VoucherLines.Any())
                throw new InvalidOperationException("Cannot post voucher without lines");

            // Validate amounts
            if (voucher.NetAmount <= 0)
                throw new InvalidOperationException("Cannot post voucher with zero or negative amount");

            // For Journal vouchers, validate debit = credit
            if (voucher.VoucherType == "Journal")
            {
                var totalDebit = voucher.VoucherLines.Sum(l => l.DebitAmount);
                var totalCredit = voucher.VoucherLines.Sum(l => l.CreditAmount);

                if (totalDebit != totalCredit)
                    throw new InvalidOperationException("Total debit and credit amounts must match for Journal vouchers");
            }
        }

        private void CalculateVoucherTotals(Voucher voucher)
        {
            voucher.TotalAmount = voucher.VoucherLines.Sum(l => l.LineAmount);
            voucher.TaxAmount = voucher.VoucherLines.Sum(l => l.TaxAmount);
            voucher.DiscountAmount = voucher.VoucherLines.Sum(l => l.DiscountAmount);
            
            // Round off to nearest rupee (can be customized)
            var grossAmount = voucher.TotalAmount + voucher.TaxAmount - voucher.DiscountAmount;
            var roundedAmount = Math.Round(grossAmount, 0);
            voucher.RoundOffAmount = roundedAmount - grossAmount;
            voucher.NetAmount = roundedAmount;
        }
    }
}
