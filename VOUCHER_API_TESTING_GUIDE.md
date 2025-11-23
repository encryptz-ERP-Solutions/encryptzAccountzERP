# Voucher API Testing Guide

## Quick Start

This guide provides sample API calls for testing the voucher CRUD endpoints.

---

## Prerequisites

1. **Authentication**: Obtain a JWT token by logging in
2. **Business ID**: Have a valid business ID
3. **Chart of Accounts**: Have at least 2 active accounts created

---

## Sample API Requests

### 1. Create a Draft Journal Voucher

```bash
POST /api/v1/businesses/{businessId}/vouchers
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "businessID": "your-business-id",
  "voucherType": "Journal",
  "voucherDate": "2025-11-21",
  "narration": "Opening balance entry",
  "lines": [
    {
      "accountID": "cash-account-id",
      "description": "Cash in hand",
      "lineAmount": 100000,
      "debitAmount": 100000,
      "creditAmount": 0
    },
    {
      "accountID": "capital-account-id",
      "description": "Owner's capital",
      "lineAmount": 100000,
      "debitAmount": 0,
      "creditAmount": 100000
    }
  ]
}
```

**Expected Response (201 Created):**
```json
{
  "voucherID": "generated-uuid",
  "businessID": "your-business-id",
  "voucherNumber": "JNL2500001",
  "voucherType": "Journal",
  "voucherDate": "2025-11-21T00:00:00Z",
  "status": "draft",
  "totalAmount": 100000,
  "taxAmount": 0,
  "netAmount": 100000,
  "narration": "Opening balance entry",
  "lines": [...]
}
```

---

### 2. Create a Sales Voucher with Tax

```bash
POST /api/v1/businesses/{businessId}/vouchers
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "businessID": "your-business-id",
  "voucherType": "Sales",
  "voucherDate": "2025-11-21",
  "partyAccountID": "customer-account-id",
  "partyName": "ABC Corporation",
  "placeOfSupply": "Maharashtra",
  "narration": "Invoice for goods sold",
  "lines": [
    {
      "accountID": "sales-account-id",
      "description": "Product A",
      "quantity": 10,
      "unitPrice": 1000,
      "taxableAmount": 10000,
      "taxRate": 18,
      "taxAmount": 1800,
      "cgstAmount": 900,
      "sgstAmount": 900,
      "lineAmount": 10000
    },
    {
      "accountID": "debtor-account-id",
      "description": "Customer payment receivable",
      "lineAmount": 11800,
      "debitAmount": 11800,
      "creditAmount": 0
    }
  ]
}
```

---

### 3. Get All Vouchers for a Business

```bash
GET /api/v1/businesses/{businessId}/vouchers
Authorization: Bearer {jwt_token}
```

**Query Parameters:**
- `status`: Filter by status (draft, posted, etc.)
- `voucherType`: Filter by type (Sales, Purchase, Journal, etc.)
- `startDate`: Start date (YYYY-MM-DD)
- `endDate`: End date (YYYY-MM-DD)

**Examples:**
```bash
# Get all draft vouchers
GET /api/v1/businesses/{businessId}/vouchers?status=draft

# Get all sales vouchers from last month
GET /api/v1/businesses/{businessId}/vouchers?voucherType=Sales&startDate=2025-10-01&endDate=2025-10-31

# Get all posted vouchers this year
GET /api/v1/businesses/{businessId}/vouchers?status=posted&startDate=2025-01-01
```

---

### 4. Get a Single Voucher

```bash
GET /api/v1/businesses/{businessId}/vouchers/{voucherId}
Authorization: Bearer {jwt_token}
```

**Expected Response (200 OK):**
```json
{
  "voucherID": "voucher-uuid",
  "businessID": "business-uuid",
  "voucherNumber": "JNL2500001",
  "voucherType": "Journal",
  "voucherDate": "2025-11-21T00:00:00Z",
  "status": "draft",
  "totalAmount": 100000,
  "taxAmount": 0,
  "discountAmount": 0,
  "roundOffAmount": 0,
  "netAmount": 100000,
  "currency": "INR",
  "exchangeRate": 1.0,
  "narration": "Opening balance entry",
  "createdAt": "2025-11-21T10:00:00Z",
  "updatedAt": "2025-11-21T10:00:00Z",
  "lines": [
    {
      "lineID": "line-uuid-1",
      "lineNumber": 1,
      "accountID": "account-uuid-1",
      "description": "Cash in hand",
      "lineAmount": 100000,
      "debitAmount": 100000,
      "creditAmount": 0
    },
    {
      "lineID": "line-uuid-2",
      "lineNumber": 2,
      "accountID": "account-uuid-2",
      "description": "Owner's capital",
      "lineAmount": 100000,
      "debitAmount": 0,
      "creditAmount": 100000
    }
  ]
}
```

---

### 5. Update a Draft Voucher

```bash
PUT /api/v1/businesses/{businessId}/vouchers/{voucherId}
Content-Type: application/json
Authorization: Bearer {jwt_token}

{
  "voucherDate": "2025-11-22",
  "narration": "Updated narration",
  "lines": [
    {
      "accountID": "cash-account-id",
      "description": "Updated description",
      "lineAmount": 150000,
      "debitAmount": 150000,
      "creditAmount": 0
    },
    {
      "accountID": "capital-account-id",
      "description": "Owner's capital - updated",
      "lineAmount": 150000,
      "debitAmount": 0,
      "creditAmount": 150000
    }
  ]
}
```

**Note:** Only draft vouchers can be updated. Attempting to update a posted voucher will return:
```json
{
  "message": "Only draft vouchers can be updated"
}
```

---

### 6. Post a Voucher

```bash
POST /api/v1/businesses/{businessId}/vouchers/{voucherId}/post
Authorization: Bearer {jwt_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Voucher posted successfully. Ledger generation pending (Step 10).",
  "voucherID": "voucher-uuid",
  "postedAt": "2025-11-21T10:30:00Z"
}
```

**Validation Errors:**

If the voucher cannot be posted, you'll receive error responses:

```json
// Already posted
{
  "message": "Only draft vouchers can be posted"
}

// No lines
{
  "message": "Cannot post voucher without lines"
}

// Zero amount
{
  "message": "Cannot post voucher with zero or negative amount"
}

// Unbalanced journal
{
  "message": "Total debit and credit amounts must match for Journal vouchers"
}
```

---

### 7. Delete a Draft Voucher

```bash
DELETE /api/v1/businesses/{businessId}/vouchers/{voucherId}
Authorization: Bearer {jwt_token}
```

**Expected Response (200 OK):**
```json
{
  "message": "Voucher deleted successfully"
}
```

**Note:** Only draft vouchers can be deleted. Attempting to delete a posted voucher will return:
```json
{
  "message": "Only draft vouchers can be deleted"
}
```

---

## Validation Rules

### Voucher Type Validation
Valid voucher types:
- `Sales`
- `Purchase`
- `Payment`
- `Receipt`
- `Journal`
- `Contra`
- `Debit Note`
- `Credit Note`

### Line Validation
- At least one line item is required
- All referenced `accountID` must exist in the chart of accounts
- All accounts must be active
- For Journal vouchers: Total debit must equal total credit

### Status Workflow
```
draft → posted (one-way transition)
```
- Only draft vouchers can be edited
- Only draft vouchers can be deleted
- Once posted, vouchers are immutable (in this step)

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Invalid voucher type. Valid types are: Sales, Purchase, Payment, Receipt, Journal, Contra, Debit Note, Credit Note"
}
```

### 401 Unauthorized
```json
{
  "message": "User ID claim is missing or invalid"
}
```

### 404 Not Found
```json
{
  "message": "Voucher not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An internal error occurred."
}
```

---

## Testing Scenarios

### Scenario 1: Happy Path - Journal Entry
1. Create a journal voucher with balanced entries ✅
2. Verify it's created with status=draft ✅
3. Update the voucher ✅
4. Post the voucher ✅
5. Verify status=posted ✅
6. Attempt to update (should fail) ✅
7. Attempt to delete (should fail) ✅

### Scenario 2: Validation Errors
1. Create voucher with invalid type (should fail) ✅
2. Create voucher with no lines (should fail) ✅
3. Create voucher with non-existent account (should fail) ✅
4. Create journal with unbalanced entries (should fail) ✅
5. Post voucher with zero amount (should fail) ✅

### Scenario 3: Sales Invoice Flow
1. Create sales voucher with customer and tax ✅
2. Add multiple line items ✅
3. Post the voucher ✅
4. Query all posted sales vouchers ✅

### Scenario 4: Filtering & Search
1. Create vouchers of different types ✅
2. Filter by status=draft ✅
3. Filter by voucherType=Sales ✅
4. Filter by date range ✅
5. Combine multiple filters ✅

---

## Postman Collection

You can import this into Postman for easier testing:

```json
{
  "info": {
    "name": "Voucher API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Create Journal Voucher",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"businessID\": \"{{businessId}}\",\n  \"voucherType\": \"Journal\",\n  \"voucherDate\": \"2025-11-21\",\n  \"narration\": \"Test voucher\",\n  \"lines\": [\n    {\n      \"accountID\": \"{{cashAccountId}}\",\n      \"lineAmount\": 1000,\n      \"debitAmount\": 1000,\n      \"creditAmount\": 0\n    },\n    {\n      \"accountID\": \"{{capitalAccountId}}\",\n      \"lineAmount\": 1000,\n      \"debitAmount\": 0,\n      \"creditAmount\": 1000\n    }\n  ]\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": {
          "raw": "{{baseUrl}}/api/v1/businesses/{{businessId}}/vouchers",
          "host": ["{{baseUrl}}"],
          "path": ["api", "v1", "businesses", "{{businessId}}", "vouchers"]
        }
      }
    }
  ]
}
```

---

## Database Queries for Verification

After testing, you can verify the data directly in PostgreSQL:

```sql
-- View all vouchers
SELECT voucher_id, voucher_number, voucher_type, status, net_amount
FROM vouchers
WHERE business_id = 'your-business-id'
ORDER BY created_at DESC;

-- View voucher lines
SELECT v.voucher_number, vl.line_number, c.account_name, 
       vl.debit_amount, vl.credit_amount, vl.line_amount
FROM vouchers v
JOIN voucher_lines vl ON v.voucher_id = vl.voucher_id
JOIN chart_of_accounts c ON vl.account_id = c.account_id
WHERE v.voucher_id = 'specific-voucher-id'
ORDER BY vl.line_number;

-- Check draft vs posted counts
SELECT status, COUNT(*) as count
FROM vouchers
WHERE business_id = 'your-business-id'
GROUP BY status;
```

---

## Next Steps

After verifying the voucher CRUD operations:

1. ✅ Test all endpoints with various scenarios
2. ✅ Verify validation rules work correctly
3. ✅ Test status workflow (draft → posted)
4. ⏭️ Proceed to Step 10: Implement full ledger generation
5. ⏭️ Add support for voucher attachments
6. ⏭️ Implement multi-level approval workflow
7. ⏭️ Add voucher templates for common transactions

---

## Support

For issues or questions about the voucher API implementation, refer to:
- `VOUCHER_IMPLEMENTATION_SUMMARY.md` for technical details
- Unit tests in `Tests/BusinessLogic.Tests/VoucherServiceTests.cs` for examples
- Database schema in `migrations/sql/2025_11_20_create_schema.sql`
