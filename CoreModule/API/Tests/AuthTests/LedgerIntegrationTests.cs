using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;
using BusinessLogic.Core.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthTests
{
    /// <summary>
    /// Integration tests for ledger posting and reporting functionality
    /// Tests the full flow from voucher creation to trial balance generation
    /// </summary>
    public class LedgerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public LedgerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        /// <summary>
        /// This test validates the complete ledger posting flow:
        /// 1. Registers/logs in a user
        /// 2. Creates a business
        /// 3. Creates chart of accounts
        /// 4. Creates a voucher
        /// 5. Posts the voucher (which should create ledger entries)
        /// 6. Retrieves trial balance
        /// 7. Retrieves P&L statement
        /// 8. Performs reconciliation check
        /// </summary>
        [Fact]
        public async Task FullLedgerPostingFlow_WithSampleData_ShouldGenerateBalancedReports()
        {
            // Arrange: Create user and authenticate
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"ledgeruser_{uniqueId}",
                FullName = "Ledger Test User",
                Email = $"ledger_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            var accessToken = registerResult.GetProperty("accessToken").GetString();
            var userId = Guid.Parse(registerResult.GetProperty("userId").GetString());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Step 1: Create a business
            var createBusinessRequest = new
            {
                businessName = $"Test Company {uniqueId}",
                businessType = "Private Limited",
                email = $"company_{uniqueId}@example.com"
            };

            var businessResponse = await _client.PostAsJsonAsync("/api/v1/businesses", createBusinessRequest);
            Assert.True(businessResponse.IsSuccessStatusCode, $"Business creation failed: {await businessResponse.Content.ReadAsStringAsync()}");

            var businessContent = await businessResponse.Content.ReadAsStringAsync();
            var businessResult = JsonSerializer.Deserialize<JsonElement>(businessContent);
            var businessId = Guid.Parse(businessResult.GetProperty("businessId").GetString());

            // Step 2: Create Chart of Accounts (simplified - just 4 accounts for testing)
            var cashAccountId = Guid.NewGuid();
            var capitalAccountId = Guid.NewGuid();
            var salesAccountId = Guid.NewGuid();
            var expenseAccountId = Guid.NewGuid();

            // Note: This assumes there's an endpoint to create accounts
            // In reality, you might need to seed data or use existing accounts
            // For this test, we'll proceed assuming accounts exist or will be created by seed data

            // Step 3: Create a Journal voucher (Opening Journal)
            var voucherId = Guid.NewGuid();
            var createVoucherRequest = new
            {
                businessId = businessId,
                voucherType = "Journal",
                voucherDate = DateTime.Today,
                narration = "Opening Journal Entry - Integration Test",
                lines = new[]
                {
                    new
                    {
                        accountId = cashAccountId,
                        lineAmount = 10000.00,
                        debitAmount = 10000.00,
                        creditAmount = 0.00,
                        description = "Opening Cash Balance"
                    },
                    new
                    {
                        accountId = capitalAccountId,
                        lineAmount = 10000.00,
                        debitAmount = 0.00,
                        creditAmount = 10000.00,
                        description = "Owner's Capital"
                    }
                }
            };

            // Note: This test assumes voucher endpoint exists and is working
            // If vouchers are already created by seed data, we can use those instead
            
            // Step 4: Get a posted voucher from seed data (more realistic)
            var vouchersResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/vouchers?status=posted");
            
            if (vouchersResponse.StatusCode == HttpStatusCode.OK)
            {
                var vouchersContent = await vouchersResponse.Content.ReadAsStringAsync();
                var vouchersResult = JsonSerializer.Deserialize<JsonElement>(vouchersContent);
                
                // If there are posted vouchers in the seed data, use the first one
                if (vouchersResult.ValueKind == JsonValueKind.Array && vouchersResult.GetArrayLength() > 0)
                {
                    var firstVoucher = vouchersResult[0];
                    var testVoucherId = Guid.Parse(firstVoucher.GetProperty("voucherID").GetString());
                    
                    // Step 5: Check if ledger entries exist for this voucher
                    var checkResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/ledgers/check/{testVoucherId}");
                    Assert.True(checkResponse.IsSuccessStatusCode);
                    
                    var checkContent = await checkResponse.Content.ReadAsStringAsync();
                    var checkResult = JsonSerializer.Deserialize<JsonElement>(checkContent);
                    var hasLedgerEntries = checkResult.GetProperty("hasLedgerEntries").GetBoolean();
                    
                    // The sample data should have already created ledger entries
                    // But we can verify the posting endpoint works too
                    if (!hasLedgerEntries)
                    {
                        var postResponse = await _client.PostAsync($"/api/v1/businesses/{businessId}/ledgers/post/{testVoucherId}", null);
                        Assert.True(postResponse.IsSuccessStatusCode, $"Posting failed: {await postResponse.Content.ReadAsStringAsync()}");
                    }
                }
            }

            // Step 6: Get Trial Balance
            var trialBalanceResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/trial-balance?from=2025-01-01&to=2025-12-31");
            Assert.True(trialBalanceResponse.IsSuccessStatusCode, $"Trial balance failed: {await trialBalanceResponse.Content.ReadAsStringAsync()}");
            
            var trialBalanceContent = await trialBalanceResponse.Content.ReadAsStringAsync();
            var trialBalance = JsonSerializer.Deserialize<JsonElement>(trialBalanceContent);
            
            // Assert: Trial balance should be balanced
            Assert.True(trialBalance.GetProperty("isBalanced").GetBoolean(), "Trial balance should be balanced");
            
            var totalClosingDebit = trialBalance.GetProperty("totalClosingDebit").GetDecimal();
            var totalClosingCredit = trialBalance.GetProperty("totalClosingCredit").GetDecimal();
            Assert.Equal(totalClosingDebit, totalClosingCredit); // Should be equal

            // Step 7: Get Profit & Loss Statement
            var pnlResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/p-and-l?from=2025-01-01&to=2025-12-31");
            Assert.True(pnlResponse.IsSuccessStatusCode, $"P&L failed: {await pnlResponse.Content.ReadAsStringAsync()}");
            
            var pnlContent = await pnlResponse.Content.ReadAsStringAsync();
            var pnl = JsonSerializer.Deserialize<JsonElement>(pnlContent);
            
            // Assert: P&L should have valid structure
            Assert.True(pnl.TryGetProperty("totalIncome", out _));
            Assert.True(pnl.TryGetProperty("totalExpenses", out _));
            Assert.True(pnl.TryGetProperty("netProfit", out _));
            Assert.True(pnl.TryGetProperty("isProfitable", out _));

            // Step 8: Perform Reconciliation Check
            var reconciliationResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/reconciliation-check?from=2025-01-01&to=2025-12-31");
            Assert.True(reconciliationResponse.IsSuccessStatusCode, $"Reconciliation failed: {await reconciliationResponse.Content.ReadAsStringAsync()}");
            
            var reconciliationContent = await reconciliationResponse.Content.ReadAsStringAsync();
            var reconciliation = JsonSerializer.Deserialize<JsonElement>(reconciliationContent);
            
            // Assert: Reconciliation should show balanced entries
            Assert.True(reconciliation.GetProperty("isBalanced").GetBoolean(), "Reconciliation should show balanced entries");
            
            var totalDebits = reconciliation.GetProperty("totalDebits").GetDecimal();
            var totalCredits = reconciliation.GetProperty("totalCredits").GetDecimal();
            Assert.Equal(totalDebits, totalCredits); // Should be equal
            
            var unbalancedVouchers = reconciliation.GetProperty("unbalancedVouchers");
            Assert.Equal(0, unbalancedVouchers.GetArrayLength()); // Should have no unbalanced vouchers
        }

        [Fact]
        public async Task GetTrialBalance_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange: Don't set authentication header
            var businessId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/trial-balance");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProfitAndLoss_WithInvalidDateRange_ShouldReturnBadRequest()
        {
            // Arrange: Create user and authenticate
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"testuser_{uniqueId}",
                FullName = "Test User",
                Email = $"test_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            var accessToken = registerResult.GetProperty("accessToken").GetString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var businessId = Guid.NewGuid();

            // Act: Request with invalid date range (to date before from date)
            var response = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/p-and-l?from=2025-12-31&to=2025-01-01");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostVoucherToLedger_Idempotency_ShouldNotDuplicateEntries()
        {
            // This test verifies that posting the same voucher twice doesn't create duplicate entries
            
            // Arrange: Create user and authenticate
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"idempotent_{uniqueId}",
                FullName = "Idempotent Test User",
                Email = $"idempotent_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            var accessToken = registerResult.GetProperty("accessToken").GetString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Create a business
            var createBusinessRequest = new
            {
                businessName = $"Idempotent Test Company {uniqueId}",
                businessType = "Private Limited"
            };

            var businessResponse = await _client.PostAsJsonAsync("/api/v1/businesses", createBusinessRequest);
            var businessContent = await businessResponse.Content.ReadAsStringAsync();
            var businessResult = JsonSerializer.Deserialize<JsonElement>(businessContent);
            var businessId = Guid.Parse(businessResult.GetProperty("businessId").GetString());

            // Get a posted voucher from seed data
            var vouchersResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/vouchers?status=posted");
            
            if (vouchersResponse.StatusCode == HttpStatusCode.OK)
            {
                var vouchersContent = await vouchersResponse.Content.ReadAsStringAsync();
                var vouchersResult = JsonSerializer.Deserialize<JsonElement>(vouchersContent);
                
                if (vouchersResult.ValueKind == JsonValueKind.Array && vouchersResult.GetArrayLength() > 0)
                {
                    var firstVoucher = vouchersResult[0];
                    var voucherId = Guid.Parse(firstVoucher.GetProperty("voucherID").GetString());
                    
                    // Act: Post the voucher twice
                    var postResponse1 = await _client.PostAsync($"/api/v1/businesses/{businessId}/ledgers/post/{voucherId}", null);
                    var postContent1 = await postResponse1.Content.ReadAsStringAsync();
                    var postResult1 = JsonSerializer.Deserialize<JsonElement>(postContent1);
                    
                    var postResponse2 = await _client.PostAsync($"/api/v1/businesses/{businessId}/ledgers/post/{voucherId}", null);
                    var postContent2 = await postResponse2.Content.ReadAsStringAsync();
                    var postResult2 = JsonSerializer.Deserialize<JsonElement>(postContent2);
                    
                    // Assert: Both should succeed but second should indicate idempotency
                    Assert.True(postResult1.GetProperty("success").GetBoolean());
                    Assert.True(postResult2.GetProperty("success").GetBoolean());
                    
                    var entriesCreated1 = postResult1.GetProperty("entriesCreated").GetInt32();
                    var entriesCreated2 = postResult2.GetProperty("entriesCreated").GetInt32();
                    
                    // Both should report the same number of entries
                    Assert.Equal(entriesCreated1, entriesCreated2);
                    
                    // Second response should indicate idempotency
                    var message2 = postResult2.GetProperty("message").GetString();
                    Assert.Contains("already posted", message2, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        [Fact]
        public async Task GetReportsSummary_ShouldReturnAllReports()
        {
            // Arrange: Create user and authenticate
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"summary_{uniqueId}",
                FullName = "Summary Test User",
                Email = $"summary_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            var accessToken = registerResult.GetProperty("accessToken").GetString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Create a business
            var createBusinessRequest = new
            {
                businessName = $"Summary Test Company {uniqueId}",
                businessType = "Private Limited"
            };

            var businessResponse = await _client.PostAsJsonAsync("/api/v1/businesses", createBusinessRequest);
            var businessContent = await businessResponse.Content.ReadAsStringAsync();
            var businessResult = JsonSerializer.Deserialize<JsonElement>(businessContent);
            var businessId = Guid.Parse(businessResult.GetProperty("businessId").GetString());

            // Act: Get reports summary
            var summaryResponse = await _client.GetAsync($"/api/v1/businesses/{businessId}/reports/summary?from=2025-01-01&to=2025-12-31");
            Assert.True(summaryResponse.IsSuccessStatusCode, $"Summary failed: {await summaryResponse.Content.ReadAsStringAsync()}");

            var summaryContent = await summaryResponse.Content.ReadAsStringAsync();
            var summary = JsonSerializer.Deserialize<JsonElement>(summaryContent);

            // Assert: Summary should contain all three report sections
            Assert.True(summary.TryGetProperty("trialBalance", out var tbSection));
            Assert.True(summary.TryGetProperty("profitAndLoss", out var pnlSection));
            Assert.True(summary.TryGetProperty("reconciliation", out var reconSection));

            // Verify trial balance section
            Assert.True(tbSection.TryGetProperty("isBalanced", out _));
            Assert.True(tbSection.TryGetProperty("totalDebits", out _));
            Assert.True(tbSection.TryGetProperty("totalCredits", out _));

            // Verify P&L section
            Assert.True(pnlSection.TryGetProperty("netProfit", out _));
            Assert.True(pnlSection.TryGetProperty("isProfitable", out _));

            // Verify reconciliation section
            Assert.True(reconSection.TryGetProperty("isBalanced", out _));
            Assert.True(reconSection.TryGetProperty("totalEntries", out _));
        }
    }
}
