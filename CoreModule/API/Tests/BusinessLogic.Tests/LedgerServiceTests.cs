using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts;
using BusinessLogic.Accounts.DTOs;
using BusinessLogic.Accounts.Mappers;
using Entities.Accounts;
using Entities.Core;
using Moq;
using Repository.Accounts;
using Repository.Core.Interface;
using Infrastructure;
using Xunit;

namespace BusinessLogic.Tests
{
    /// <summary>
    /// Unit tests for LedgerService - focusing on posting logic and report generation
    /// </summary>
    public class LedgerServiceTests
    {
        private readonly Mock<ILedgerRepository> _mockLedgerRepository;
        private readonly Mock<IVoucherRepository> _mockVoucherRepository;
        private readonly Mock<IChartOfAccountRepository> _mockChartOfAccountRepository;
        private readonly Mock<IBusinessRepository> _mockBusinessRepository;
        private readonly Mock<CoreSQLDbHelper> _mockSqlHelper;
        private readonly IMapper _mapper;
        private readonly ILedgerService _ledgerService;

        public LedgerServiceTests()
        {
            _mockLedgerRepository = new Mock<ILedgerRepository>();
            _mockVoucherRepository = new Mock<IVoucherRepository>();
            _mockChartOfAccountRepository = new Mock<IChartOfAccountRepository>();
            _mockBusinessRepository = new Mock<IBusinessRepository>();
            _mockSqlHelper = new Mock<CoreSQLDbHelper>(MockBehavior.Loose, (string)null);

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LedgerMappingProfile>();
                cfg.AddProfile<VoucherMappingProfile>();
            });
            _mapper = config.CreateMapper();

            _ledgerService = new LedgerService(
                _mockLedgerRepository.Object,
                _mockVoucherRepository.Object,
                _mockChartOfAccountRepository.Object,
                _mockBusinessRepository.Object,
                _mockSqlHelper.Object,
                _mapper
            );
        }

        #region PostVoucherToLedgerAsync Tests

        [Fact]
        public async Task PostVoucherToLedgerAsync_WithVoucherNotFound_ShouldReturnFailure()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync((Voucher)null);

            // Act
            var result = await _ledgerService.PostVoucherToLedgerAsync(voucherId, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Voucher not found", result.Message);
            Assert.Equal(0, result.EntriesCreated);
        }

        [Fact]
        public async Task PostVoucherToLedgerAsync_WithAlreadyPostedVoucher_ShouldReturnIdempotent()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var businessId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                Status = "posted",
                VoucherLines = new List<VoucherLine>
                {
                    new VoucherLine { DebitAmount = 100, CreditAmount = 0 }
                }
            };

            var existingEntries = new List<LedgerEntry>
            {
                new LedgerEntry { EntryID = Guid.NewGuid(), VoucherID = voucherId }
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);
            _mockLedgerRepository.Setup(r => r.HasEntriesForVoucherAsync(voucherId))
                .ReturnsAsync(true);
            _mockLedgerRepository.Setup(r => r.GetByVoucherIdAsync(voucherId))
                .ReturnsAsync(existingEntries);

            // Act
            var result = await _ledgerService.PostVoucherToLedgerAsync(voucherId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("already posted", result.Message);
            Assert.Equal(1, result.EntriesCreated);
        }

        [Fact]
        public async Task PostVoucherToLedgerAsync_WithNonPostedStatus_ShouldReturnFailure()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var businessId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                Status = "draft",
                VoucherLines = new List<VoucherLine>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);
            _mockLedgerRepository.Setup(r => r.HasEntriesForVoucherAsync(voucherId))
                .ReturnsAsync(false);

            // Act
            var result = await _ledgerService.PostVoucherToLedgerAsync(voucherId, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("must be in 'posted' status", result.Message);
        }

        [Fact]
        public async Task PostVoucherToLedgerAsync_WithNoLines_ShouldReturnFailure()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var businessId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                Status = "posted",
                VoucherLines = new List<VoucherLine>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);
            _mockLedgerRepository.Setup(r => r.HasEntriesForVoucherAsync(voucherId))
                .ReturnsAsync(false);

            // Act
            var result = await _ledgerService.PostVoucherToLedgerAsync(voucherId, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("no lines to post", result.Message);
        }

        #endregion

        #region GetLedgerStatementAsync Tests

        [Fact]
        public async Task GetLedgerStatementAsync_WithValidAccount_ShouldReturnStatement()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var businessId = Guid.NewGuid();
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var account = new ChartOfAccount
            {
                AccountID = accountId,
                BusinessID = businessId,
                AccountCode = "1000",
                AccountName = "Cash",
                AccountType = "Asset"
            };

            var entries = new List<LedgerEntry>
            {
                new LedgerEntry
                {
                    EntryID = Guid.NewGuid(),
                    AccountID = accountId,
                    EntryDate = new DateTime(2025, 1, 15),
                    DebitAmount = 1000,
                    CreditAmount = 0
                },
                new LedgerEntry
                {
                    EntryID = Guid.NewGuid(),
                    AccountID = accountId,
                    EntryDate = new DateTime(2025, 1, 20),
                    DebitAmount = 0,
                    CreditAmount = 500
                }
            };

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync(account);
            _mockLedgerRepository.Setup(r => r.GetAccountBalanceAsync(accountId, It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            _mockLedgerRepository.Setup(r => r.GetByAccountIdAsync(accountId, fromDate, toDate))
                .ReturnsAsync(entries);

            // Act
            var result = await _ledgerService.GetLedgerStatementAsync(accountId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.AccountID);
            Assert.Equal("Cash", result.AccountName);
            Assert.Equal(1000, result.TotalDebits);
            Assert.Equal(500, result.TotalCredits);
            Assert.Equal(2, result.Entries.Count);
        }

        [Fact]
        public async Task GetLedgerStatementAsync_WithInvalidAccount_ShouldThrowException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var fromDate = DateTime.Today;
            var toDate = DateTime.Today;

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync((ChartOfAccount)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _ledgerService.GetLedgerStatementAsync(accountId, fromDate, toDate));
        }

        #endregion

        #region GetTrialBalanceAsync Tests

        [Fact]
        public async Task GetTrialBalanceAsync_WithValidData_ShouldReturnBalancedTrialBalance()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var business = new Business
            {
                BusinessID = businessId,
                BusinessName = "Test Company"
            };

            var accounts = new List<ChartOfAccount>
            {
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "1000",
                    AccountName = "Cash",
                    AccountType = "Asset",
                    IsActive = true,
                    IsGroup = false
                },
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "3000",
                    AccountName = "Capital",
                    AccountType = "Equity",
                    IsActive = true,
                    IsGroup = false
                }
            };

            var entries = new List<LedgerEntry>
            {
                new LedgerEntry
                {
                    AccountID = accounts[0].AccountID,
                    EntryDate = fromDate,
                    DebitAmount = 10000,
                    CreditAmount = 0
                },
                new LedgerEntry
                {
                    AccountID = accounts[1].AccountID,
                    EntryDate = fromDate,
                    DebitAmount = 0,
                    CreditAmount = 10000
                }
            };

            _mockBusinessRepository.Setup(r => r.GetByIdAsync(businessId))
                .ReturnsAsync(business);
            _mockChartOfAccountRepository.Setup(r => r.GetAllByBusinessIdAsync(businessId))
                .ReturnsAsync(accounts);
            _mockLedgerRepository.Setup(r => r.GetAccountBalancesAsync(businessId, It.IsAny<DateTime>()))
                .ReturnsAsync(new Dictionary<Guid, decimal>());
            _mockLedgerRepository.Setup(r => r.GetByBusinessIdAsync(businessId, fromDate, toDate))
                .ReturnsAsync(entries);

            // Act
            var result = await _ledgerService.GetTrialBalanceAsync(businessId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(businessId, result.BusinessID);
            Assert.Equal("Test Company", result.BusinessName);
            Assert.True(result.IsBalanced);
            Assert.Equal(10000, result.TotalPeriodDebit);
            Assert.Equal(10000, result.TotalPeriodCredit);
        }

        #endregion

        #region GetProfitAndLossAsync Tests

        [Fact]
        public async Task GetProfitAndLossAsync_WithProfitableData_ShouldReturnPositiveNetProfit()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var business = new Business
            {
                BusinessID = businessId,
                BusinessName = "Test Company"
            };

            var accounts = new List<ChartOfAccount>
            {
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "4000",
                    AccountName = "Sales Revenue",
                    AccountType = "Revenue",
                    IsActive = true,
                    IsGroup = false
                },
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "5000",
                    AccountName = "Operating Expenses",
                    AccountType = "Expense",
                    IsActive = true,
                    IsGroup = false
                }
            };

            var entries = new List<LedgerEntry>
            {
                new LedgerEntry
                {
                    AccountID = accounts[0].AccountID, // Revenue
                    EntryDate = fromDate,
                    DebitAmount = 0,
                    CreditAmount = 50000 // Income is credit
                },
                new LedgerEntry
                {
                    AccountID = accounts[1].AccountID, // Expense
                    EntryDate = fromDate,
                    DebitAmount = 30000, // Expense is debit
                    CreditAmount = 0
                }
            };

            _mockBusinessRepository.Setup(r => r.GetByIdAsync(businessId))
                .ReturnsAsync(business);
            _mockChartOfAccountRepository.Setup(r => r.GetAllByBusinessIdAsync(businessId))
                .ReturnsAsync(accounts);
            _mockLedgerRepository.Setup(r => r.GetByBusinessIdAsync(businessId, fromDate, toDate))
                .ReturnsAsync(entries);

            // Act
            var result = await _ledgerService.GetProfitAndLossAsync(businessId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(businessId, result.BusinessID);
            Assert.Equal(50000, result.TotalIncome);
            Assert.Equal(30000, result.TotalExpenses);
            Assert.Equal(20000, result.NetProfit);
            Assert.True(result.IsProfitable);
        }

        [Fact]
        public async Task GetProfitAndLossAsync_WithLossData_ShouldReturnNegativeNetProfit()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var business = new Business
            {
                BusinessID = businessId,
                BusinessName = "Test Company"
            };

            var accounts = new List<ChartOfAccount>
            {
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "4000",
                    AccountName = "Sales Revenue",
                    AccountType = "Revenue",
                    IsActive = true,
                    IsGroup = false
                },
                new ChartOfAccount
                {
                    AccountID = Guid.NewGuid(),
                    BusinessID = businessId,
                    AccountCode = "5000",
                    AccountName = "Operating Expenses",
                    AccountType = "Expense",
                    IsActive = true,
                    IsGroup = false
                }
            };

            var entries = new List<LedgerEntry>
            {
                new LedgerEntry
                {
                    AccountID = accounts[0].AccountID, // Revenue
                    EntryDate = fromDate,
                    DebitAmount = 0,
                    CreditAmount = 20000
                },
                new LedgerEntry
                {
                    AccountID = accounts[1].AccountID, // Expense
                    EntryDate = fromDate,
                    DebitAmount = 35000,
                    CreditAmount = 0
                }
            };

            _mockBusinessRepository.Setup(r => r.GetByIdAsync(businessId))
                .ReturnsAsync(business);
            _mockChartOfAccountRepository.Setup(r => r.GetAllByBusinessIdAsync(businessId))
                .ReturnsAsync(accounts);
            _mockLedgerRepository.Setup(r => r.GetByBusinessIdAsync(businessId, fromDate, toDate))
                .ReturnsAsync(entries);

            // Act
            var result = await _ledgerService.GetProfitAndLossAsync(businessId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20000, result.TotalIncome);
            Assert.Equal(35000, result.TotalExpenses);
            Assert.Equal(-15000, result.NetProfit);
            Assert.False(result.IsProfitable);
        }

        #endregion

        #region GetReconciliationCheckAsync Tests

        [Fact]
        public async Task GetReconciliationCheckAsync_WithBalancedEntries_ShouldReturnBalanced()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var business = new Business
            {
                BusinessID = businessId,
                BusinessName = "Test Company"
            };

            var entries = new List<LedgerEntry>
            {
                new LedgerEntry
                {
                    VoucherID = Guid.NewGuid(),
                    DebitAmount = 1000,
                    CreditAmount = 0
                },
                new LedgerEntry
                {
                    VoucherID = Guid.NewGuid(),
                    DebitAmount = 0,
                    CreditAmount = 1000
                }
            };

            _mockBusinessRepository.Setup(r => r.GetByIdAsync(businessId))
                .ReturnsAsync(business);
            _mockLedgerRepository.Setup(r => r.GetTotalsForPeriodAsync(businessId, fromDate, toDate))
                .ReturnsAsync((1000m, 1000m));
            _mockLedgerRepository.Setup(r => r.GetByBusinessIdAsync(businessId, fromDate, toDate))
                .ReturnsAsync(entries);

            // Act
            var result = await _ledgerService.GetReconciliationCheckAsync(businessId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsBalanced);
            Assert.Equal(1000, result.TotalDebits);
            Assert.Equal(1000, result.TotalCredits);
            Assert.Equal(0, result.Difference);
            Assert.Equal(2, result.TotalEntries);
        }

        #endregion

        #region HasLedgerEntriesAsync Tests

        [Fact]
        public async Task HasLedgerEntriesAsync_WithExistingEntries_ShouldReturnTrue()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            _mockLedgerRepository.Setup(r => r.HasEntriesForVoucherAsync(voucherId))
                .ReturnsAsync(true);

            // Act
            var result = await _ledgerService.HasLedgerEntriesAsync(voucherId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasLedgerEntriesAsync_WithNoEntries_ShouldReturnFalse()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            _mockLedgerRepository.Setup(r => r.HasEntriesForVoucherAsync(voucherId))
                .ReturnsAsync(false);

            // Act
            var result = await _ledgerService.HasLedgerEntriesAsync(voucherId);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
