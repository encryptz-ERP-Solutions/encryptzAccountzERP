# Ledger Posting & Reporting - Quick Start Guide

## Prerequisites
- PostgreSQL database running
- .NET 6.0 or later
- Sample data loaded (optional but recommended)

## Step 1: Apply Database Schema

The ledger_entries table and indexes already exist in the schema:

```bash
cd /workspace/migrations/sql
psql -U postgres -d your_database -f 2025_11_20_create_schema.sql
```

## Step 2: Load Sample Data (Recommended)

```bash
psql -U postgres -d your_database -f 2025_11_20_sample_data.sql
```

This creates:
- Sample business: `44444444-4444-4444-4444-000000000001`
- Admin user: `admin@encryptz.com` / `Admin@123`
- Chart of accounts with common accounts
- 3 sample vouchers (already posted)
- Ledger entries for those vouchers

## Step 3: Start the API

```bash
cd /workspace/CoreModule/API/encryptzERP
dotnet run
```

The API will be available at `http://localhost:5000` (or configured port).

## Step 4: Authenticate

### Using Sample Data:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUserHandle": "admin@encryptz.com",
    "password": "Admin@123"
  }'
```

Save the `accessToken` from the response.

### Or Register New User:
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userHandle": "testuser",
    "fullName": "Test User",
    "email": "test@example.com",
    "password": "Test@1234"
  }'
```

## Step 5: Test Ledger Endpoints

Replace `{TOKEN}` with your access token and `{BUSINESS_ID}` with your business ID.

### Check if Voucher is Posted

```bash
curl -X GET "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/ledgers/check/{VOUCHER_ID}" \
  -H "Authorization: Bearer {TOKEN}"
```

### Get Trial Balance

```bash
curl -X GET "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/reports/trial-balance?from=2025-01-01&to=2025-12-31" \
  -H "Authorization: Bearer {TOKEN}"
```

### Get Profit & Loss

```bash
curl -X GET "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/reports/p-and-l?from=2025-01-01&to=2025-12-31" \
  -H "Authorization: Bearer {TOKEN}"
```

### Get Reconciliation Check

```bash
curl -X GET "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/reports/reconciliation-check?from=2025-01-01&to=2025-12-31" \
  -H "Authorization: Bearer {TOKEN}"
```

### Get Reports Summary

```bash
curl -X GET "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/reports/summary?from=2025-01-01&to=2025-12-31" \
  -H "Authorization: Bearer {TOKEN}"
```

## Step 6: Test Voucher Posting

### Create a Voucher (if not using sample data)

```bash
curl -X POST "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/vouchers" \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "businessId": "{BUSINESS_ID}",
    "voucherType": "Journal",
    "voucherDate": "2025-01-15",
    "narration": "Test Journal Entry",
    "lines": [
      {
        "accountId": "{CASH_ACCOUNT_ID}",
        "lineAmount": 1000.00,
        "debitAmount": 1000.00,
        "creditAmount": 0.00,
        "description": "Debit Cash"
      },
      {
        "accountId": "{CAPITAL_ACCOUNT_ID}",
        "lineAmount": 1000.00,
        "debitAmount": 0.00,
        "creditAmount": 1000.00,
        "description": "Credit Capital"
      }
    ]
  }'
```

### Post the Voucher

```bash
curl -X POST "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/vouchers/{VOUCHER_ID}/post" \
  -H "Authorization: Bearer {TOKEN}"
```

This will:
1. Update voucher status to "posted"
2. Automatically create ledger entries
3. Return the number of entries created

### Manual Ledger Posting (if automatic failed)

```bash
curl -X POST "http://localhost:5000/api/v1/businesses/{BUSINESS_ID}/ledgers/post/{VOUCHER_ID}" \
  -H "Authorization: Bearer {TOKEN}"
```

## Step 7: Run Tests

### Unit Tests

```bash
cd /workspace/CoreModule/API/Tests/BusinessLogic.Tests
dotnet test
```

### Integration Tests

```bash
cd /workspace/CoreModule/API/Tests/AuthTests
dotnet test --filter "FullyQualifiedName~LedgerIntegrationTests"
```

## Using with Sample Data

The sample data includes:

**Business ID:** `44444444-4444-4444-4444-000000000001`

**Sample Vouchers:**
- Opening Journal: `88888888-8888-8888-8888-000000000001`
- Sales Invoice: `88888888-8888-8888-8888-000000000002`
- Payment Receipt: `88888888-8888-8888-8888-000000000003`

**Example - Get Trial Balance for Sample Business:**

```bash
# First, login
TOKEN=$(curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUserHandle":"admin@encryptz.com","password":"Admin@123"}' \
  | jq -r '.accessToken')

# Then get trial balance
curl -X GET "http://localhost:5000/api/v1/businesses/44444444-4444-4444-4444-000000000001/reports/trial-balance?from=2025-01-01&to=2025-12-31" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.'
```

## Expected Results

### Trial Balance
- Should be balanced (totalClosingDebit = totalClosingCredit)
- Shows opening, period, and closing balances
- Includes all accounts with activity

### Profit & Loss
- Shows revenue accounts (credit side)
- Shows expense accounts (debit side)
- Calculates net profit (income - expenses)
- Indicates if profitable

### Reconciliation Check
- Validates total debits = total credits
- Shows entry and voucher counts
- Lists any unbalanced vouchers (should be empty)

## Swagger UI

Access interactive API documentation:

```
http://localhost:5000/swagger
```

Look for:
- `LedgerController` - Ledger operations
- `ReportsController` - Financial reports

## Troubleshooting

### "Voucher must be in 'posted' status"
- Ensure you call POST `/vouchers/{id}/post` first
- Check voucher status: GET `/vouchers/{id}`

### "Trial balance is not balanced"
- Run reconciliation check to find unbalanced vouchers
- Verify all vouchers have matching debits and credits

### "Unauthorized"
- Ensure you're passing the Bearer token
- Token format: `Authorization: Bearer {token}`
- Check if token has expired (tokens expire after configured time)

### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Ensure database exists and schema is applied

## Next Steps

1. **Create More Vouchers**: Test with sales, purchases, payments
2. **Generate Reports**: Try different date ranges
3. **Test Idempotency**: Post same voucher twice, verify no duplicates
4. **Explore Ledger Statements**: View account-wise transactions
5. **Build UI**: Use these APIs to build a frontend

## API Reference

Full documentation available in:
- `/workspace/LEDGER_IMPLEMENTATION_SUMMARY.md` - Complete implementation guide
- Swagger UI - Interactive API documentation
- Integration tests - Real usage examples

## Support

For issues or questions:
1. Check `/workspace/LEDGER_IMPLEMENTATION_SUMMARY.md`
2. Review unit and integration tests for examples
3. Enable detailed logging in `appsettings.json`

## Key Endpoints Summary

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/businesses/{id}/ledgers/{accountId}` | GET | Account ledger statement |
| `/businesses/{id}/ledgers/post/{voucherId}` | POST | Manual ledger posting |
| `/businesses/{id}/ledgers/check/{voucherId}` | GET | Check if posted |
| `/businesses/{id}/reports/trial-balance` | GET | Trial balance |
| `/businesses/{id}/reports/p-and-l` | GET | Profit & Loss |
| `/businesses/{id}/reports/reconciliation-check` | GET | Reconciliation status |
| `/businesses/{id}/reports/summary` | GET | Dashboard summary |

All endpoints require authentication via Bearer token.
