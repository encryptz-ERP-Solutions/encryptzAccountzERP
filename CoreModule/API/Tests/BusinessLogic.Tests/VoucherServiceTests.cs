using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts;
using BusinessLogic.Accounts.DTOs;
using BusinessLogic.Accounts.Mappers;
using Entities.Accounts;
using Moq;
using Repository.Accounts;
using Xunit;

namespace BusinessLogic.Tests
{
    /// <summary>
    /// Unit tests for VoucherService - focusing on validation and create flow
    /// </summary>
    public class VoucherServiceTests
    {
        private readonly Mock<IVoucherRepository> _mockVoucherRepository;
        private readonly Mock<IChartOfAccountRepository> _mockChartOfAccountRepository;
        private readonly IMapper _mapper;
        private readonly IVoucherService _voucherService;

        public VoucherServiceTests()
        {
            _mockVoucherRepository = new Mock<IVoucherRepository>();
            _mockChartOfAccountRepository = new Mock<IChartOfAccountRepository>();

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<VoucherMappingProfile>();
            });
            _mapper = config.CreateMapper();

            _voucherService = new VoucherService(
                _mockVoucherRepository.Object,
                _mockChartOfAccountRepository.Object,
                _mapper
            );
        }

        #region CreateVoucherAsync Tests

        [Fact]
        public async Task CreateVoucherAsync_WithValidData_ShouldCreateVoucher()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var account = new ChartOfAccount
            {
                AccountID = accountId,
                BusinessID = businessId,
                AccountCode = "1000",
                AccountName = "Cash",
                IsActive = true
            };

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "Journal",
                VoucherDate = DateTime.Today,
                Narration = "Test voucher",
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto
                    {
                        AccountID = accountId,
                        LineAmount = 1000,
                        DebitAmount = 1000,
                        CreditAmount = 0
                    },
                    new CreateVoucherLineDto
                    {
                        AccountID = accountId,
                        LineAmount = 1000,
                        DebitAmount = 0,
                        CreditAmount = 1000
                    }
                }
            };

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync(account);
            _mockVoucherRepository.Setup(r => r.GenerateVoucherNumberAsync(businessId, "Journal"))
                .ReturnsAsync("JNL2500001");
            _mockVoucherRepository.Setup(r => r.CreateAsync(It.IsAny<Voucher>()))
                .ReturnsAsync((Voucher v) => v);
            _mockVoucherRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => new Voucher { VoucherID = id, VoucherLines = new List<VoucherLine>() });

            // Act
            var result = await _voucherService.CreateVoucherAsync(createDto, userId);

            // Assert
            Assert.NotNull(result);
            _mockVoucherRepository.Verify(r => r.CreateAsync(It.IsAny<Voucher>()), Times.Once);
            _mockVoucherRepository.Verify(r => r.GenerateVoucherNumberAsync(businessId, "Journal"), Times.Once);
        }

        [Fact]
        public async Task CreateVoucherAsync_WithInvalidVoucherType_ShouldThrowArgumentException()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "InvalidType",
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto { AccountID = accountId, LineAmount = 1000 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _voucherService.CreateVoucherAsync(createDto, userId));
        }

        [Fact]
        public async Task CreateVoucherAsync_WithNoLines_ShouldThrowArgumentException()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "Journal",
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _voucherService.CreateVoucherAsync(createDto, userId));
            Assert.Contains("At least one voucher line is required", exception.Message);
        }

        [Fact]
        public async Task CreateVoucherAsync_WithInvalidAccountID_ShouldThrowArgumentException()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "Journal",
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto { AccountID = accountId, LineAmount = 1000 }
                }
            };

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync((ChartOfAccount?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _voucherService.CreateVoucherAsync(createDto, userId));
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task CreateVoucherAsync_WithInactiveAccount_ShouldThrowArgumentException()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var account = new ChartOfAccount
            {
                AccountID = accountId,
                BusinessID = businessId,
                AccountCode = "1000",
                AccountName = "Cash",
                IsActive = false
            };

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "Journal",
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto { AccountID = accountId, LineAmount = 1000 }
                }
            };

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _voucherService.CreateVoucherAsync(createDto, userId));
            Assert.Contains("not active", exception.Message);
        }

        [Fact]
        public async Task CreateVoucherAsync_JournalWithUnbalancedEntries_ShouldThrowArgumentException()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var account = new ChartOfAccount
            {
                AccountID = accountId,
                BusinessID = businessId,
                AccountCode = "1000",
                AccountName = "Cash",
                IsActive = true
            };

            var createDto = new CreateVoucherDto
            {
                BusinessID = businessId,
                VoucherType = "Journal",
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto
                    {
                        AccountID = accountId,
                        LineAmount = 1000,
                        DebitAmount = 1000,
                        CreditAmount = 0
                    },
                    new CreateVoucherLineDto
                    {
                        AccountID = accountId,
                        LineAmount = 500,
                        DebitAmount = 0,
                        CreditAmount = 500
                    }
                }
            };

            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _voucherService.CreateVoucherAsync(createDto, userId));
            Assert.Contains("must match", exception.Message);
        }

        #endregion

        #region UpdateVoucherAsync Tests

        [Fact]
        public async Task UpdateVoucherAsync_DraftVoucher_ShouldUpdateSuccessfully()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var businessId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingVoucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                VoucherNumber = "JNL2500001",
                VoucherType = "Journal",
                Status = "draft",
                VoucherLines = new List<VoucherLine>()
            };

            var account = new ChartOfAccount
            {
                AccountID = accountId,
                BusinessID = businessId,
                AccountCode = "1000",
                AccountName = "Cash",
                IsActive = true
            };

            var updateDto = new UpdateVoucherDto
            {
                VoucherDate = DateTime.Today,
                Narration = "Updated narration",
                Lines = new List<CreateVoucherLineDto>
                {
                    new CreateVoucherLineDto { AccountID = accountId, LineAmount = 2000 }
                }
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(existingVoucher);
            _mockChartOfAccountRepository.Setup(r => r.GetChartOfAccountByIdAsync(accountId))
                .ReturnsAsync(account);
            _mockVoucherRepository.Setup(r => r.UpdateAsync(It.IsAny<Voucher>()))
                .ReturnsAsync(true);

            // Act
            var result = await _voucherService.UpdateVoucherAsync(voucherId, updateDto, userId);

            // Assert
            Assert.NotNull(result);
            _mockVoucherRepository.Verify(r => r.UpdateAsync(It.IsAny<Voucher>()), Times.Once);
        }

        [Fact]
        public async Task UpdateVoucherAsync_PostedVoucher_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var businessId = Guid.NewGuid();

            var existingVoucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                VoucherNumber = "JNL2500001",
                VoucherType = "Journal",
                Status = "posted",
                VoucherLines = new List<VoucherLine>()
            };

            var updateDto = new UpdateVoucherDto
            {
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(existingVoucher);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.UpdateVoucherAsync(voucherId, updateDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateVoucherAsync_NonExistentVoucher_ShouldThrowArgumentException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var updateDto = new UpdateVoucherDto
            {
                VoucherDate = DateTime.Today,
                Lines = new List<CreateVoucherLineDto>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync((Voucher?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _voucherService.UpdateVoucherAsync(voucherId, updateDto, Guid.NewGuid()));
        }

        #endregion

        #region DeleteVoucherAsync Tests

        [Fact]
        public async Task DeleteVoucherAsync_DraftVoucher_ShouldDeleteSuccessfully()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
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
            _mockVoucherRepository.Setup(r => r.DeleteAsync(voucherId))
                .ReturnsAsync(true);

            // Act
            var result = await _voucherService.DeleteVoucherAsync(voucherId);

            // Assert
            Assert.True(result);
            _mockVoucherRepository.Verify(r => r.DeleteAsync(voucherId), Times.Once);
        }

        [Fact]
        public async Task DeleteVoucherAsync_PostedVoucher_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
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

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.DeleteVoucherAsync(voucherId));
        }

        #endregion

        #region PostVoucherAsync Tests

        [Fact]
        public async Task PostVoucherAsync_ValidDraftVoucher_ShouldPostSuccessfully()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var businessId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                BusinessID = businessId,
                VoucherNumber = "JNL2500001",
                VoucherType = "Journal",
                Status = "draft",
                NetAmount = 1000,
                VoucherLines = new List<VoucherLine>
                {
                    new VoucherLine
                    {
                        LineID = Guid.NewGuid(),
                        AccountID = Guid.NewGuid(),
                        LineAmount = 1000,
                        DebitAmount = 1000,
                        CreditAmount = 0
                    },
                    new VoucherLine
                    {
                        LineID = Guid.NewGuid(),
                        AccountID = Guid.NewGuid(),
                        LineAmount = 1000,
                        DebitAmount = 0,
                        CreditAmount = 1000
                    }
                }
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);
            _mockVoucherRepository.Setup(r => r.PostVoucherAsync(voucherId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _voucherService.PostVoucherAsync(voucherId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(voucherId, result.VoucherID);
            _mockVoucherRepository.Verify(r => r.PostVoucherAsync(voucherId, userId), Times.Once);
        }

        [Fact]
        public async Task PostVoucherAsync_PostedVoucher_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                Status = "posted",
                VoucherLines = new List<VoucherLine>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.PostVoucherAsync(voucherId, Guid.NewGuid()));
        }

        [Fact]
        public async Task PostVoucherAsync_VoucherWithNoLines_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                Status = "draft",
                NetAmount = 1000,
                VoucherLines = new List<VoucherLine>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.PostVoucherAsync(voucherId, Guid.NewGuid()));
            Assert.Contains("without lines", exception.Message);
        }

        [Fact]
        public async Task PostVoucherAsync_VoucherWithZeroAmount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                Status = "draft",
                NetAmount = 0,
                VoucherLines = new List<VoucherLine>
                {
                    new VoucherLine { LineID = Guid.NewGuid(), AccountID = Guid.NewGuid() }
                }
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.PostVoucherAsync(voucherId, Guid.NewGuid()));
            Assert.Contains("zero or negative", exception.Message);
        }

        [Fact]
        public async Task PostVoucherAsync_JournalWithUnbalancedEntries_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            var voucher = new Voucher
            {
                VoucherID = voucherId,
                VoucherType = "Journal",
                Status = "draft",
                NetAmount = 1000,
                VoucherLines = new List<VoucherLine>
                {
                    new VoucherLine
                    {
                        LineID = Guid.NewGuid(),
                        AccountID = Guid.NewGuid(),
                        DebitAmount = 1000,
                        CreditAmount = 0
                    },
                    new VoucherLine
                    {
                        LineID = Guid.NewGuid(),
                        AccountID = Guid.NewGuid(),
                        DebitAmount = 0,
                        CreditAmount = 500
                    }
                }
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _voucherService.PostVoucherAsync(voucherId, Guid.NewGuid()));
            Assert.Contains("must match", exception.Message);
        }

        #endregion

        #region GetVoucherByIdAsync Tests

        [Fact]
        public async Task GetVoucherByIdAsync_ExistingVoucher_ShouldReturnVoucher()
        {
            // Arrange
            var voucherId = Guid.NewGuid();
            var voucher = new Voucher
            {
                VoucherID = voucherId,
                VoucherNumber = "JNL2500001",
                VoucherLines = new List<VoucherLine>()
            };

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync(voucher);

            // Act
            var result = await _voucherService.GetVoucherByIdAsync(voucherId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(voucherId, result.VoucherID);
        }

        [Fact]
        public async Task GetVoucherByIdAsync_NonExistentVoucher_ShouldReturnNull()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            _mockVoucherRepository.Setup(r => r.GetByIdAsync(voucherId))
                .ReturnsAsync((Voucher?)null);

            // Act
            var result = await _voucherService.GetVoucherByIdAsync(voucherId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetVouchersByBusinessIdAsync Tests

        [Fact]
        public async Task GetVouchersByBusinessIdAsync_ShouldReturnVouchers()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var vouchers = new List<Voucher>
            {
                new Voucher { VoucherID = Guid.NewGuid(), BusinessID = businessId, VoucherLines = new List<VoucherLine>() },
                new Voucher { VoucherID = Guid.NewGuid(), BusinessID = businessId, VoucherLines = new List<VoucherLine>() }
            };

            _mockVoucherRepository.Setup(r => r.GetAllByBusinessIdAsync(businessId, null, null, null, null))
                .ReturnsAsync(vouchers);

            // Act
            var result = await _voucherService.GetVouchersByBusinessIdAsync(businessId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetVouchersByBusinessIdAsync_WithFilters_ShouldPassFiltersToRepository()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var status = "posted";
            var voucherType = "Sales";
            var startDate = DateTime.Today.AddDays(-30);
            var endDate = DateTime.Today;

            _mockVoucherRepository.Setup(r => r.GetAllByBusinessIdAsync(
                businessId, status, voucherType, startDate, endDate))
                .ReturnsAsync(new List<Voucher>());

            // Act
            await _voucherService.GetVouchersByBusinessIdAsync(businessId, status, voucherType, startDate, endDate);

            // Assert
            _mockVoucherRepository.Verify(r => r.GetAllByBusinessIdAsync(
                businessId, status, voucherType, startDate, endDate), Times.Once);
        }

        #endregion
    }
}
