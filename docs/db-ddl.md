# Database Schema Documentation

## Overview

This document explains the complete database schema for the ERP system. The schema is designed with multi-tenancy, financial precision, and compliance in mind.

## Design Principles

### 1. UUID Primary Keys
All tables use UUID primary keys (`uuid_generate_v4()`) instead of integer sequences for:
- **Distributed System Support**: No sequence conflicts across multiple instances
- **Security**: Non-sequential IDs prevent enumeration attacks
- **Merge-Friendly**: No conflicts when merging data from different sources

### 2. Audit Columns
Every business table includes standard audit columns:
- `created_at`: Timestamp with timezone (UTC)
- `updated_at`: Timestamp with timezone (UTC)
- `created_by`: UUID reference to users table
- `updated_by`: UUID reference to users table

### 3. Financial Precision
- **Monetary Amounts**: `NUMERIC(18,2)` - Stores up to 999,999,999,999,999.99
- **Tax Rates**: `NUMERIC(18,4)` - Supports rates like 2.5000%, 28.0000%
- **Exchange Rates**: `NUMERIC(18,6)` - Precision for currency conversions
- **Quantities**: `NUMERIC(18,2)` - Supports fractional quantities

### 4. Timezone Awareness
All timestamps use `TIMESTAMPTZ` to:
- Store values in UTC automatically
- Support multi-region deployments
- Handle daylight saving time correctly

### 5. Multi-Tenancy
Business isolation is enforced through:
- Every business table has `business_id` foreign key
- Indexes on `(business_id, other_columns)` for performance
- Application-level tenant filtering

### 6. Soft Deletes
Most tables use `is_active` boolean instead of hard deletes:
- Preserves audit trail
- Allows data recovery
- Maintains referential integrity

### 7. Extensibility
JSONB columns for flexible schema evolution:
- `metadata`: Custom fields without schema changes
- `settings`: Configuration options
- `json_data`: Complex nested structures (GST/TDS data)

---

## Schema Modules

### Module 1: Identity & Authentication

#### users
Core user identity table with authentication details.

**Key Features**:
- Email-based authentication with unique constraint
- Phone verification support
- Account lockout mechanism (`failed_login_attempts`, `locked_until`)
- System admin flag for super users
- Password change tracking

**Security Notes**:
- Store only password hashes (bcrypt, argon2, etc.)
- Never log or expose password_hash
- Implement rate limiting on failed login attempts

#### refresh_tokens
JWT refresh token management for secure session handling.

**Key Features**:
- Token revocation support
- IP address and user agent tracking
- Automatic expiration
- Audit trail for token revocation

**Best Practices**:
- Set reasonable expiration (7-30 days)
- Clean up expired tokens regularly
- Rotate tokens on password change

---

### Module 2: Roles & Permissions (RBAC)

#### roles
Role definitions for both system-wide and business-specific access control.

**Key Features**:
- System roles (e.g., Super Admin) vs. business roles (e.g., Accountant)
- Hierarchical structure via `business_id`
- Permissions stored as JSONB array for flexibility

#### permissions
Granular permission catalog.

**Common Permissions**:
```
- accounts.view
- accounts.create
- accounts.edit
- accounts.delete
- vouchers.approve
- reports.export
- settings.manage
```

#### user_roles & role_permissions
Many-to-many relationships for flexible access control.

**Access Control Flow**:
1. User logs in → Load user_roles
2. For each role → Load role_permissions
3. For each permission → Check permission_code
4. Grant access if permission exists

---

### Module 3: Businesses & Subscriptions

#### businesses
Multi-tenant organization management.

**Key Features**:
- Complete business profile (legal name, tax IDs)
- Indian tax registration (GSTIN, PAN, TAN)
- Address and contact information
- Verification workflow
- Business settings and metadata

**Tax Fields**:
- `gstin`: 15-character GST Identification Number (e.g., 27AABCU9603R1ZX)
- `pan`: 10-character Permanent Account Number
- `tan`: 10-character Tax Deduction Account Number

#### business_users
User-business relationship with access control.

**Key Features**:
- Many-to-many: Users can belong to multiple businesses
- Owner designation
- Status tracking (active, inactive, suspended, invited)
- Invitation workflow support

**Status Flow**:
```
invited → active → inactive/suspended
         ↑_________(rejoin)
```

#### subscription_plans & subscriptions
SaaS subscription management.

**Billing Cycles**:
- Monthly
- Quarterly
- Yearly
- Lifetime

**Status Flow**:
```
trial → active → past_due → cancelled
              ↓            ↓
            expired      expired
```

**Features & Limits**:
Store in JSONB for flexibility:
```json
{
  "features": ["multi-user", "gst-filing", "inventory"],
  "limits": {
    "max_users": 10,
    "max_transactions": 10000,
    "storage_gb": 5
  }
}
```

---

### Module 4: Chart of Accounts & Tax

#### chart_of_accounts
Hierarchical account structure for double-entry bookkeeping.

**Account Types**:
1. **Asset**: Cash, Bank, Inventory, Fixed Assets, Receivables
2. **Liability**: Payables, Loans, Credit Cards
3. **Equity**: Capital, Reserves, Retained Earnings
4. **Revenue**: Sales, Services, Other Income
5. **Expense**: Purchases, Salaries, Rent, Utilities

**Account Hierarchy**:
```
Assets (level 1, is_group=true)
  ├─ Current Assets (level 2, is_group=true)
  │   ├─ Cash (level 3, is_group=false)
  │   └─ Bank Accounts (level 3, is_group=true)
  │       ├─ HDFC Savings (level 4, is_group=false)
  │       └─ ICICI Current (level 4, is_group=false)
  └─ Fixed Assets (level 2, is_group=true)
      ├─ Furniture (level 3, is_group=false)
      └─ Computers (level 3, is_group=false)
```

**Key Features**:
- Parent-child relationships via `parent_account_id`
- Opening balance tracking
- Current balance (updated by ledger entries)
- System accounts (cannot be deleted)

#### tax_codes
Tax rate configurations for GST, TDS, TCS compliance.

**Indian Tax Types**:
- **GST**: Goods and Services Tax (standard)
- **CGST**: Central GST (intra-state)
- **SGST**: State GST (intra-state)
- **IGST**: Integrated GST (inter-state)
- **UTGST**: Union Territory GST
- **TDS**: Tax Deducted at Source
- **TCS**: Tax Collected at Source

**Example Tax Rates**:
```sql
-- GST rates
5.00%   -- Essential goods
12.00%  -- Standard goods
18.00%  -- Most services
28.00%  -- Luxury goods

-- TDS rates
0.10%   -- 194Q - Purchase of goods
2.00%   -- 194C - Contractor payments
10.00%  -- 194J - Professional fees
```

**Effective Dates**:
- `effective_from`: Tax rate start date
- `effective_to`: Tax rate end date (NULL for current rates)
- Supports historical tax rate changes

---

### Module 5: Inventory & Warehousing

#### warehouses
Storage location management.

**Key Features**:
- Multiple warehouses per business
- Complete address and contact information
- Warehouse types (main, branch, godown, virtual)

#### items
Product and service catalog with pricing and stock tracking.

**Item Types**:
- **goods**: Physical products (tracked inventory)
- **service**: Services (no inventory)
- **asset**: Capital goods
- **component**: Raw materials/components

**Pricing**:
- `purchase_price`: Last purchase price
- `selling_price`: Default selling price
- `mrp`: Maximum Retail Price
- `cost_price`: Weighted average cost

**Stock Management**:
- `current_stock`: Real-time stock quantity
- `minimum_stock`: Reorder level alert
- `maximum_stock`: Maximum stock limit
- `reorder_level`: Auto-order trigger

**HSN/SAC Codes**:
- `hsn_code`: Harmonized System of Nomenclature (goods)
- `sac_code`: Service Accounting Code (services)
- Required for GST compliance

#### inventory_movements
Complete stock movement tracking.

**Movement Types**:
- **receipt**: Stock received (purchase, production)
- **issue**: Stock issued (sales, consumption)
- **transfer**: Inter-warehouse transfer
- **adjustment**: Stock adjustment (physical count)
- **return**: Sales/purchase return
- **opening**: Opening stock entry

**Valuation**:
- `unit_cost`: Cost per unit
- `total_value`: Quantity × Unit Cost
- `stock_before`: Stock before this movement
- `stock_after`: Stock after this movement

**Batch & Serial Tracking**:
- `batch_number`: For batch-tracked items
- `serial_number`: For serialized items
- `expiry_date`: For perishable goods

---

### Module 6: Accounting Transactions

#### vouchers
Transaction header for all accounting entries.

**Voucher Types**:
1. **Sales**: Sales invoices (revenue)
2. **Purchase**: Purchase invoices (expense)
3. **Payment**: Payment out (bank/cash)
4. **Receipt**: Payment in (bank/cash)
5. **Journal**: Manual journal entries
6. **Contra**: Bank-to-bank transfers
7. **Debit Note**: Purchase returns
8. **Credit Note**: Sales returns

**Status Flow**:
```
draft → pending → approved → posted
                    ↓           ↓
                cancelled    void
```

**Amount Fields**:
- `total_amount`: Sum of line amounts before tax
- `tax_amount`: Total tax (GST, TDS, etc.)
- `discount_amount`: Total discount
- `round_off_amount`: Rounding adjustment
- `net_amount`: Final amount to pay/receive

**GST Fields**:
- `party_gstin`: Customer/supplier GSTIN
- `place_of_supply`: State name (determines tax type)
- `is_reverse_charge`: RCM applicable
- `is_bill_of_supply`: Unregistered dealer

#### voucher_lines
Line items with item, account, quantity, and tax details.

**Key Features**:
- Each line maps to an account
- Optional item reference for inventory items
- Quantity and unit price for items
- Multi-level discounts
- Tax calculation per line

**Tax Breakdown**:
- `taxable_amount`: Amount before tax
- `tax_rate`: GST rate percentage
- `cgst_amount`: Central GST (intra-state)
- `sgst_amount`: State GST (intra-state)
- `igst_amount`: Integrated GST (inter-state)
- `cess_amount`: Additional cess if applicable

**Accounting Fields**:
- `debit_amount`: Debit side (assets, expenses)
- `credit_amount`: Credit side (liabilities, revenue)

#### ledger_entries
Posted accounting entries for financial reports.

**Key Principles**:
- Created automatically when voucher is posted
- One entry per line per account
- Supports multi-currency with exchange rates
- Immutable after creation (audit trail)

**Double-Entry Bookkeeping**:
```
Every transaction must balance:
Σ debit_amount = Σ credit_amount
```

**Reconciliation**:
- `reconciliation_status`: unreconciled, reconciled, discrepancy
- `reconciled_at`: Reconciliation timestamp
- `reconciled_by`: User who reconciled
- Used for bank reconciliation

---

### Module 7: Registers & Compliance

#### registers
Statutory register management for compliance.

**Register Types**:
1. **GSTR1**: Outward supplies (sales)
2. **GSTR3B**: Monthly summary return
3. **Purchase**: Purchase register
4. **Sales**: Sales register
5. **TDS**: TDS deduction register
6. **TCS**: TCS collection register
7. **Inventory**: Stock register

**Period Management**:
- `period_from`: Start date
- `period_to`: End date
- `financial_year`: E.g., "2024-25"

**Status**:
- **open**: Entries can be added/edited
- **locked**: No changes allowed
- **filed**: Return filed with government
- **revised**: Revised return filed

#### gst_submissions
GST return filing details and calculations.

**Return Types**:
- **GSTR1**: Outward supplies (monthly/quarterly)
- **GSTR3B**: Summary return (monthly)
- **GSTR9**: Annual return
- **GSTR9C**: Reconciliation statement (audited)
- **CMP08**: Composition scheme return

**Tax Calculation**:
```
Output Tax (Sales) - Input Tax (Purchases) = Net Tax Payable

Split into:
- IGST Payable
- CGST Payable
- SGST Payable
- Cess Payable
```

**Payment Tracking**:
- `payment_status`: pending, paid, partial
- `payment_date`: Date of payment
- `payment_reference`: Challan number

#### tds_submissions
TDS return filing for quarterly submissions.

**Form Types**:
- **24Q**: TDS on salaries
- **26Q**: TDS on payments other than salaries
- **27Q**: TDS on non-resident payments
- **27EQ**: TCS quarterly return

**Quarter Format**:
- Q1: Apr-Jun
- Q2: Jul-Sep
- Q3: Oct-Dec
- Q4: Jan-Mar

**Challan Details**:
Store as JSONB array:
```json
[
  {
    "challan_number": "0099999999",
    "bsr_code": "0123456",
    "date": "2024-07-07",
    "amount": 50000.00
  }
]
```

---

### Module 8: Audit & Logging

#### audit_logs
Complete audit trail of all system actions.

**Actions Tracked**:
- **CREATE**: New record created
- **READ**: Sensitive data accessed
- **UPDATE**: Record modified
- **DELETE**: Record deleted/deactivated
- **LOGIN/LOGOUT**: Authentication events
- **EXPORT**: Data export operations
- **APPROVE/REJECT**: Workflow actions
- **POST/VOID**: Financial posting actions

**Change Tracking**:
- `old_values`: Previous field values (JSONB)
- `new_values`: New field values (JSONB)
- `changes`: Diff of changes (JSONB)

**Example Change Log**:
```json
{
  "old_values": {"status": "draft", "amount": 1000.00},
  "new_values": {"status": "posted", "amount": 1000.00},
  "changes": {"status": {"from": "draft", "to": "posted"}}
}
```

**Security Context**:
- `ip_address`: Client IP address
- `user_agent`: Browser/app identifier
- `session_id`: Session identifier
- `request_id`: Request trace ID

---

## Indexes Strategy

### Index Design Principles

1. **Tenant Isolation**: Every business table has index on `business_id` first
2. **Date Ranges**: Financial queries frequently filter by date ranges
3. **Status Filters**: Active/inactive, posted/draft statuses
4. **Lookup Fields**: Unique codes, email, phone, GSTIN
5. **Foreign Keys**: All FK columns indexed for join performance
6. **Partial Indexes**: Use WHERE clause for filtered indexes (saves space)

### High-Impact Indexes

**Most Frequently Used**:
```sql
-- Account ledger queries (used in every report)
idx_ledger_account ON ledger_entries(account_id, entry_date DESC)

-- Voucher lookups
idx_vouchers_date ON vouchers(business_id, voucher_date DESC)

-- Chart of accounts filtering
idx_coa_business_code ON chart_of_accounts(business_id, account_code)

-- Item lookups in transactions
idx_items_business_code ON items(business_id, item_code)

-- User authentication
idx_users_email ON users(email)
```

### Index Maintenance

```sql
-- Check index usage
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY pg_relation_size(indexrelid) DESC;

-- Rebuild bloated indexes
REINDEX INDEX CONCURRENTLY idx_name;

-- Analyze table statistics
ANALYZE table_name;
```

---

## Sample Queries

### Query 1: Trial Balance

Trial Balance shows all accounts with their debit and credit totals for a period.

```sql
SELECT 
    coa.account_code,
    coa.account_name,
    coa.account_type,
    COALESCE(SUM(le.debit_amount), 0) AS total_debit,
    COALESCE(SUM(le.credit_amount), 0) AS total_credit,
    COALESCE(SUM(le.debit_amount), 0) - COALESCE(SUM(le.credit_amount), 0) AS balance
FROM 
    chart_of_accounts coa
LEFT JOIN 
    ledger_entries le ON coa.account_id = le.account_id
    AND le.entry_date BETWEEN '2024-04-01' AND '2024-03-31'
    AND le.business_id = 'business-uuid-here'
WHERE 
    coa.business_id = 'business-uuid-here'
    AND coa.is_active = TRUE
    AND coa.is_group = FALSE  -- Only leaf accounts
GROUP BY 
    coa.account_code, coa.account_name, coa.account_type
HAVING 
    COALESCE(SUM(le.debit_amount), 0) != 0 
    OR COALESCE(SUM(le.credit_amount), 0) != 0
ORDER BY 
    coa.account_code;
```

**Validation**: Sum of all debits must equal sum of all credits.

---

### Query 2: Profit & Loss Statement

P&L shows revenue minus expenses for a period.

```sql
WITH account_totals AS (
    SELECT 
        coa.account_type,
        coa.account_name,
        coa.account_code,
        CASE 
            WHEN coa.account_type = 'Revenue' 
            THEN COALESCE(SUM(le.credit_amount - le.debit_amount), 0)
            WHEN coa.account_type = 'Expense'
            THEN COALESCE(SUM(le.debit_amount - le.credit_amount), 0)
        END AS amount
    FROM 
        chart_of_accounts coa
    LEFT JOIN 
        ledger_entries le ON coa.account_id = le.account_id
        AND le.entry_date BETWEEN '2024-04-01' AND '2024-12-31'
        AND le.business_id = 'business-uuid-here'
    WHERE 
        coa.business_id = 'business-uuid-here'
        AND coa.account_type IN ('Revenue', 'Expense')
        AND coa.is_active = TRUE
        AND coa.is_group = FALSE
    GROUP BY 
        coa.account_type, coa.account_name, coa.account_code
),
summary AS (
    SELECT 
        SUM(CASE WHEN account_type = 'Revenue' THEN amount ELSE 0 END) AS total_revenue,
        SUM(CASE WHEN account_type = 'Expense' THEN amount ELSE 0 END) AS total_expense
    FROM account_totals
)
SELECT 
    'Revenue' AS section,
    account_name,
    account_code,
    amount
FROM account_totals
WHERE account_type = 'Revenue'

UNION ALL

SELECT 
    'Total Revenue' AS section,
    '' AS account_name,
    '' AS account_code,
    total_revenue
FROM summary

UNION ALL

SELECT 
    'Expenses' AS section,
    account_name,
    account_code,
    amount
FROM account_totals
WHERE account_type = 'Expense'

UNION ALL

SELECT 
    'Total Expenses' AS section,
    '' AS account_name,
    '' AS account_code,
    total_expense
FROM summary

UNION ALL

SELECT 
    'Net Profit' AS section,
    '' AS account_name,
    '' AS account_code,
    total_revenue - total_expense
FROM summary;
```

---

### Query 3: Balance Sheet

Balance Sheet shows financial position (Assets = Liabilities + Equity).

```sql
WITH opening_balances AS (
    SELECT 
        coa.account_id,
        coa.account_type,
        coa.account_name,
        coa.account_code,
        coa.opening_balance AS opening,
        COALESCE(SUM(
            CASE 
                WHEN coa.account_type IN ('Asset', 'Expense') 
                THEN le.debit_amount - le.credit_amount
                ELSE le.credit_amount - le.debit_amount
            END
        ), 0) AS movement,
        coa.opening_balance + COALESCE(SUM(
            CASE 
                WHEN coa.account_type IN ('Asset', 'Expense') 
                THEN le.debit_amount - le.credit_amount
                ELSE le.credit_amount - le.debit_amount
            END
        ), 0) AS closing_balance
    FROM 
        chart_of_accounts coa
    LEFT JOIN 
        ledger_entries le ON coa.account_id = le.account_id
        AND le.entry_date <= '2024-12-31'
        AND le.business_id = 'business-uuid-here'
    WHERE 
        coa.business_id = 'business-uuid-here'
        AND coa.account_type IN ('Asset', 'Liability', 'Equity')
        AND coa.is_active = TRUE
        AND coa.is_group = FALSE
    GROUP BY 
        coa.account_id, coa.account_type, coa.account_name, 
        coa.account_code, coa.opening_balance
)
SELECT 
    account_type AS section,
    account_name,
    account_code,
    closing_balance
FROM opening_balances
WHERE closing_balance != 0
ORDER BY 
    CASE account_type
        WHEN 'Asset' THEN 1
        WHEN 'Liability' THEN 2
        WHEN 'Equity' THEN 3
    END,
    account_code;
```

**Verification**:
```sql
SELECT 
    SUM(CASE WHEN account_type = 'Asset' THEN closing_balance ELSE 0 END) AS total_assets,
    SUM(CASE WHEN account_type = 'Liability' THEN closing_balance ELSE 0 END) AS total_liabilities,
    SUM(CASE WHEN account_type = 'Equity' THEN closing_balance ELSE 0 END) AS total_equity
FROM opening_balances;

-- Must satisfy: total_assets = total_liabilities + total_equity
```

---

### Query 4: Account Ledger

Detailed transaction history for a specific account.

```sql
SELECT 
    le.entry_date,
    v.voucher_type,
    v.voucher_number,
    v.narration,
    le.debit_amount,
    le.credit_amount,
    SUM(le.debit_amount - le.credit_amount) OVER (
        ORDER BY le.entry_date, le.created_at
    ) AS running_balance
FROM 
    ledger_entries le
JOIN 
    vouchers v ON le.voucher_id = v.voucher_id
WHERE 
    le.account_id = 'account-uuid-here'
    AND le.business_id = 'business-uuid-here'
    AND le.entry_date BETWEEN '2024-04-01' AND '2024-12-31'
ORDER BY 
    le.entry_date, le.created_at;
```

---

### Query 5: Outstanding Invoices (Receivables)

Unpaid sales invoices with aging analysis.

```sql
WITH invoice_payments AS (
    SELECT 
        sales.voucher_id,
        COALESCE(SUM(payment_lines.credit_amount), 0) AS paid_amount
    FROM 
        vouchers sales
    LEFT JOIN 
        voucher_lines payment_lines ON payment_lines.metadata->>'invoice_id' = sales.voucher_id::text
    WHERE 
        sales.business_id = 'business-uuid-here'
        AND sales.voucher_type = 'Sales'
        AND sales.status = 'posted'
    GROUP BY sales.voucher_id
)
SELECT 
    v.voucher_number AS invoice_number,
    v.voucher_date AS invoice_date,
    v.party_name AS customer_name,
    v.due_date,
    v.net_amount AS invoice_amount,
    COALESCE(ip.paid_amount, 0) AS paid_amount,
    v.net_amount - COALESCE(ip.paid_amount, 0) AS outstanding_amount,
    CURRENT_DATE - v.due_date AS days_overdue,
    CASE 
        WHEN CURRENT_DATE - v.due_date <= 0 THEN 'Not Due'
        WHEN CURRENT_DATE - v.due_date <= 30 THEN '0-30 days'
        WHEN CURRENT_DATE - v.due_date <= 60 THEN '31-60 days'
        WHEN CURRENT_DATE - v.due_date <= 90 THEN '61-90 days'
        ELSE '90+ days'
    END AS aging_bucket
FROM 
    vouchers v
LEFT JOIN 
    invoice_payments ip ON v.voucher_id = ip.voucher_id
WHERE 
    v.business_id = 'business-uuid-here'
    AND v.voucher_type = 'Sales'
    AND v.status = 'posted'
    AND v.net_amount - COALESCE(ip.paid_amount, 0) > 0
ORDER BY 
    v.due_date;
```

---

### Query 6: GST Summary (GSTR-3B)

Monthly GST summary for GSTR-3B filing.

```sql
WITH outward_supplies AS (
    SELECT 
        SUM(vl.taxable_amount) AS taxable_value,
        SUM(vl.igst_amount) AS igst,
        SUM(vl.cgst_amount) AS cgst,
        SUM(vl.sgst_amount) AS sgst,
        SUM(vl.cess_amount) AS cess
    FROM 
        vouchers v
    JOIN 
        voucher_lines vl ON v.voucher_id = vl.voucher_id
    WHERE 
        v.business_id = 'business-uuid-here'
        AND v.voucher_type IN ('Sales', 'Debit Note')
        AND v.status = 'posted'
        AND v.voucher_date BETWEEN '2024-04-01' AND '2024-04-30'
),
inward_supplies AS (
    SELECT 
        SUM(vl.taxable_amount) AS taxable_value,
        SUM(vl.igst_amount) AS igst,
        SUM(vl.cgst_amount) AS cgst,
        SUM(vl.sgst_amount) AS sgst,
        SUM(vl.cess_amount) AS cess
    FROM 
        vouchers v
    JOIN 
        voucher_lines vl ON v.voucher_id = vl.voucher_id
    WHERE 
        v.business_id = 'business-uuid-here'
        AND v.voucher_type IN ('Purchase', 'Credit Note')
        AND v.status = 'posted'
        AND v.voucher_date BETWEEN '2024-04-01' AND '2024-04-30'
)
SELECT 
    'Outward Supplies' AS category,
    os.taxable_value,
    os.igst,
    os.cgst,
    os.sgst,
    os.cess,
    os.igst + os.cgst + os.sgst + os.cess AS total_tax
FROM outward_supplies os

UNION ALL

SELECT 
    'Inward Supplies' AS category,
    ins.taxable_value,
    ins.igst,
    ins.cgst,
    ins.sgst,
    ins.cess,
    ins.igst + ins.cgst + ins.sgst + ins.cess AS total_tax
FROM inward_supplies ins

UNION ALL

SELECT 
    'Net Tax Payable' AS category,
    os.taxable_value - ins.taxable_value AS taxable_value,
    os.igst - ins.igst AS igst,
    os.cgst - ins.cgst AS cgst,
    os.sgst - ins.sgst AS sgst,
    os.cess - ins.cess AS cess,
    (os.igst + os.cgst + os.sgst + os.cess) - 
    (ins.igst + ins.cgst + ins.sgst + ins.cess) AS total_tax
FROM outward_supplies os, inward_supplies ins;
```

---

### Query 7: Stock Valuation Report

Current stock with valuation by warehouse.

```sql
WITH latest_stock AS (
    SELECT 
        im.item_id,
        im.warehouse_id,
        im.stock_after AS current_stock,
        ROW_NUMBER() OVER (
            PARTITION BY im.item_id, im.warehouse_id 
            ORDER BY im.movement_date DESC, im.created_at DESC
        ) AS rn
    FROM 
        inventory_movements im
    WHERE 
        im.business_id = 'business-uuid-here'
)
SELECT 
    w.warehouse_name,
    i.item_code,
    i.item_name,
    i.unit_of_measure,
    ls.current_stock,
    i.cost_price,
    ls.current_stock * i.cost_price AS stock_value,
    i.minimum_stock,
    CASE 
        WHEN ls.current_stock <= i.minimum_stock THEN 'Reorder'
        WHEN ls.current_stock <= i.reorder_level THEN 'Low Stock'
        ELSE 'OK'
    END AS stock_status
FROM 
    items i
JOIN 
    latest_stock ls ON i.item_id = ls.item_id AND ls.rn = 1
JOIN 
    warehouses w ON ls.warehouse_id = w.warehouse_id
WHERE 
    i.business_id = 'business-uuid-here'
    AND i.is_stock_tracked = TRUE
    AND i.is_active = TRUE
    AND ls.current_stock > 0
ORDER BY 
    w.warehouse_name, i.item_code;
```

---

### Query 8: Top Selling Items

Best-selling items by quantity and revenue.

```sql
SELECT 
    i.item_code,
    i.item_name,
    i.category,
    COUNT(DISTINCT v.voucher_id) AS invoice_count,
    SUM(vl.quantity) AS total_quantity_sold,
    SUM(vl.taxable_amount) AS total_revenue,
    AVG(vl.unit_price) AS avg_selling_price
FROM 
    vouchers v
JOIN 
    voucher_lines vl ON v.voucher_id = vl.voucher_id
JOIN 
    items i ON vl.item_id = i.item_id
WHERE 
    v.business_id = 'business-uuid-here'
    AND v.voucher_type = 'Sales'
    AND v.status = 'posted'
    AND v.voucher_date BETWEEN '2024-04-01' AND '2024-12-31'
GROUP BY 
    i.item_id, i.item_code, i.item_name, i.category
ORDER BY 
    total_revenue DESC
LIMIT 20;
```

---

### Query 9: User Activity Report

Track user actions for security and compliance.

```sql
SELECT 
    u.email,
    u.first_name || ' ' || u.last_name AS full_name,
    al.action,
    al.entity_type,
    al.entity_id,
    al.timestamp,
    al.ip_address,
    CASE 
        WHEN al.old_values IS NOT NULL THEN 'Modified'
        ELSE 'Created/Accessed'
    END AS change_type
FROM 
    audit_logs al
JOIN 
    users u ON al.user_id = u.user_id
WHERE 
    al.business_id = 'business-uuid-here'
    AND al.timestamp BETWEEN '2024-04-01' AND '2024-04-30'
ORDER BY 
    al.timestamp DESC
LIMIT 100;
```

---

### Query 10: Subscription Revenue Report (SaaS Metrics)

Track subscription revenue and churn.

```sql
WITH subscription_metrics AS (
    SELECT 
        DATE_TRUNC('month', s.starts_at) AS month,
        COUNT(*) AS total_subscriptions,
        COUNT(*) FILTER (WHERE s.status = 'trial') AS trial_count,
        COUNT(*) FILTER (WHERE s.status = 'active') AS active_count,
        COUNT(*) FILTER (WHERE s.status = 'cancelled') AS cancelled_count,
        SUM(s.amount) FILTER (WHERE s.status = 'active') AS mrr,
        AVG(s.amount) FILTER (WHERE s.status = 'active') AS avg_revenue_per_user
    FROM 
        subscriptions s
    GROUP BY 
        DATE_TRUNC('month', s.starts_at)
)
SELECT 
    TO_CHAR(month, 'Mon YYYY') AS month,
    total_subscriptions,
    trial_count,
    active_count,
    cancelled_count,
    ROUND(mrr, 2) AS monthly_recurring_revenue,
    ROUND(avg_revenue_per_user, 2) AS arpu,
    ROUND(cancelled_count::NUMERIC / NULLIF(active_count, 0) * 100, 2) AS churn_rate_pct
FROM 
    subscription_metrics
ORDER BY 
    month DESC;
```

---

## Performance Optimization

### Query Performance Tips

1. **Always Filter by business_id First**:
   ```sql
   WHERE business_id = 'uuid' AND other_conditions
   ```
   This leverages multi-tenant indexes and reduces scan size.

2. **Use Date Ranges Carefully**:
   ```sql
   WHERE entry_date BETWEEN '2024-04-01' AND '2024-03-31'
   ```
   Indexed date columns perform well with range queries.

3. **Avoid SELECT ***:
   ```sql
   SELECT account_id, account_name, balance  -- Good
   SELECT *  -- Bad for performance
   ```

4. **Use EXISTS Instead of IN for Large Sets**:
   ```sql
   WHERE EXISTS (SELECT 1 FROM ...)  -- Faster
   WHERE account_id IN (SELECT ...)  -- Slower
   ```

5. **Leverage Partial Indexes**:
   ```sql
   WHERE is_active = TRUE  -- Uses partial index
   ```

### Table Partitioning

For high-volume tables, consider partitioning:

#### Partition audit_logs by Month
```sql
CREATE TABLE audit_logs_2024_04 PARTITION OF audit_logs
FOR VALUES FROM ('2024-04-01') TO ('2024-05-01');

CREATE TABLE audit_logs_2024_05 PARTITION OF audit_logs
FOR VALUES FROM ('2024-05-01') TO ('2024-06-01');
```

#### Partition ledger_entries by Year
```sql
CREATE TABLE ledger_entries_2024 PARTITION OF ledger_entries
FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');

CREATE TABLE ledger_entries_2025 PARTITION OF ledger_entries
FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
```

### Maintenance Tasks

#### Daily Tasks
```sql
-- Vacuum frequently updated tables
VACUUM ANALYZE vouchers, ledger_entries, inventory_movements;
```

#### Weekly Tasks
```sql
-- Rebuild critical indexes
REINDEX INDEX CONCURRENTLY idx_ledger_account;
REINDEX INDEX CONCURRENTLY idx_vouchers_date;

-- Update statistics
ANALYZE chart_of_accounts, items, tax_codes;
```

#### Monthly Tasks
```sql
-- Archive old audit logs (older than 1 year)
INSERT INTO audit_logs_archive 
SELECT * FROM audit_logs 
WHERE timestamp < NOW() - INTERVAL '1 year';

DELETE FROM audit_logs 
WHERE timestamp < NOW() - INTERVAL '1 year';

-- Clean up expired refresh tokens
DELETE FROM refresh_tokens 
WHERE expires_at < NOW() - INTERVAL '30 days';
```

---

## Security Best Practices

### 1. Row-Level Security (RLS)

Enable RLS for multi-tenant isolation:

```sql
ALTER TABLE vouchers ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON vouchers
FOR ALL
TO authenticated_user
USING (business_id = current_setting('app.current_business_id')::uuid);
```

### 2. Encrypted Columns

For sensitive data, use pgcrypto:

```sql
CREATE EXTENSION pgcrypto;

-- Encrypt sensitive fields
UPDATE users 
SET metadata = metadata || jsonb_build_object(
    'pan_encrypted', 
    pgp_sym_encrypt('ABCDE1234F', 'encryption_key')
);
```

### 3. Audit Logging

Log all sensitive operations:

```sql
CREATE OR REPLACE FUNCTION audit_trigger_func()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_logs (
        entity_type, entity_id, action, 
        old_values, new_values, user_id
    ) VALUES (
        TG_TABLE_NAME, 
        NEW.id, 
        TG_OP,
        row_to_json(OLD),
        row_to_json(NEW),
        current_setting('app.current_user_id')::uuid
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to sensitive tables
CREATE TRIGGER audit_chart_of_accounts
AFTER INSERT OR UPDATE OR DELETE ON chart_of_accounts
FOR EACH ROW EXECUTE FUNCTION audit_trigger_func();
```

### 4. Access Control

Never expose database directly. Always use application middleware:
- Validate business_id matches user's access
- Check user permissions before queries
- Use prepared statements to prevent SQL injection
- Rate limit API endpoints

---

## Data Seeding

### Post-Migration Setup

After running the migration, seed these tables:

#### 1. System Permissions
```sql
INSERT INTO permissions (permission_code, permission_name, module) VALUES
('accounts.view', 'View Accounts', 'accounting'),
('accounts.create', 'Create Accounts', 'accounting'),
('accounts.edit', 'Edit Accounts', 'accounting'),
('accounts.delete', 'Delete Accounts', 'accounting'),
('vouchers.view', 'View Vouchers', 'accounting'),
('vouchers.create', 'Create Vouchers', 'accounting'),
('vouchers.edit', 'Edit Vouchers', 'accounting'),
('vouchers.approve', 'Approve Vouchers', 'accounting'),
('vouchers.post', 'Post Vouchers', 'accounting'),
('reports.view', 'View Reports', 'reporting'),
('reports.export', 'Export Reports', 'reporting'),
('settings.manage', 'Manage Settings', 'admin');
```

#### 2. System Roles
```sql
INSERT INTO roles (role_name, is_system_role, permissions) VALUES
('Super Admin', TRUE, '["*"]'),
('Business Owner', FALSE, '["accounts.*", "vouchers.*", "reports.*", "settings.*"]'),
('Accountant', FALSE, '["accounts.view", "vouchers.*", "reports.view"]'),
('Auditor', FALSE, '["accounts.view", "vouchers.view", "reports.*"]');
```

#### 3. Default Subscription Plans
```sql
INSERT INTO subscription_plans (plan_name, plan_code, price, features) VALUES
('Free', 'FREE', 0, '["basic-accounting", "1-user"]'),
('Starter', 'STARTER', 999, '["accounting", "gst", "5-users"]'),
('Professional', 'PRO', 2999, '["accounting", "gst", "tds", "inventory", "20-users"]'),
('Enterprise', 'ENTERPRISE', 9999, '["all-features", "unlimited-users"]');
```

#### 4. Standard Chart of Accounts Template
```sql
-- This is a simplified example; full CoA has 50+ accounts
INSERT INTO chart_of_accounts (business_id, account_code, account_name, account_type, is_group) VALUES
('template', '1000', 'Assets', 'Asset', TRUE),
('template', '1100', 'Current Assets', 'Asset', TRUE),
('template', '1101', 'Cash', 'Asset', FALSE),
('template', '1102', 'Bank Accounts', 'Asset', FALSE),
('template', '2000', 'Liabilities', 'Liability', TRUE),
('template', '3000', 'Equity', 'Equity', TRUE),
('template', '4000', 'Revenue', 'Revenue', TRUE),
('template', '5000', 'Expenses', 'Expense', TRUE);
```

---

## Troubleshooting

### Common Issues

#### Issue 1: UUID Extension Not Enabled
**Error**: `function uuid_generate_v4() does not exist`

**Solution**:
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

#### Issue 2: Trial Balance Not Balancing
**Symptoms**: Sum of debits ≠ Sum of credits

**Diagnosis**:
```sql
SELECT 
    SUM(debit_amount) AS total_debit,
    SUM(credit_amount) AS total_credit,
    SUM(debit_amount) - SUM(credit_amount) AS difference
FROM ledger_entries
WHERE business_id = 'uuid';
```

**Common Causes**:
- Voucher posted without balanced lines
- Manual ledger entry without double-entry
- Currency conversion errors

#### Issue 3: Slow Queries
**Symptoms**: Reports taking > 5 seconds

**Diagnosis**:
```sql
EXPLAIN ANALYZE
SELECT ... FROM ledger_entries WHERE ...;
```

**Solutions**:
- Add missing indexes
- Partition large tables
- Use materialized views for complex reports
- Cache frequently-run reports

#### Issue 4: Stock Discrepancies
**Symptoms**: Stock on hand ≠ System stock

**Diagnosis**:
```sql
-- Check for missing movements
SELECT item_id, COUNT(*) 
FROM inventory_movements 
WHERE stock_after IS NULL 
GROUP BY item_id;

-- Recalculate stock
SELECT 
    item_id,
    SUM(CASE WHEN movement_type IN ('receipt', 'return') THEN quantity ELSE -quantity END) AS calculated_stock,
    MAX(stock_after) AS system_stock
FROM inventory_movements
GROUP BY item_id;
```

---

## Migration Checklist

### Pre-Migration
- [ ] Backup existing database
- [ ] Enable uuid-ossp extension
- [ ] Review disk space (estimate 2x current size)
- [ ] Schedule maintenance window
- [ ] Test migration on staging environment

### During Migration
- [ ] Run migration script
- [ ] Verify all tables created
- [ ] Check all indexes created
- [ ] Verify foreign key constraints
- [ ] Run data validation queries

### Post-Migration
- [ ] Seed system data (permissions, roles, plans)
- [ ] Create initial business and users
- [ ] Run sample queries to verify
- [ ] Check audit logs working
- [ ] Monitor query performance
- [ ] Update application connection strings
- [ ] Run integration tests

### Performance Baseline
- [ ] Record query execution times
- [ ] Monitor table sizes
- [ ] Check index usage
- [ ] Set up monitoring alerts

---

## Appendix

### Entity Relationship Diagram (ERD)

```
users
  ├─ refresh_tokens
  ├─ user_roles → roles → role_permissions → permissions
  └─ business_users → businesses
                         ├─ subscriptions → subscription_plans
                         ├─ chart_of_accounts
                         ├─ tax_codes
                         ├─ warehouses
                         ├─ items
                         ├─ inventory_movements
                         ├─ vouchers
                         │    ├─ voucher_lines
                         │    └─ ledger_entries
                         ├─ registers
                         ├─ gst_submissions
                         ├─ tds_submissions
                         └─ audit_logs
```

### Glossary

- **Chart of Accounts (CoA)**: Hierarchical list of all financial accounts
- **Double-Entry Bookkeeping**: Every transaction has equal debits and credits
- **GSTIN**: GST Identification Number (15 characters)
- **HSN**: Harmonized System of Nomenclature (for goods)
- **SAC**: Service Accounting Code (for services)
- **TDS**: Tax Deducted at Source
- **TCS**: Tax Collected at Source
- **Trial Balance**: Report showing all account balances to verify debits = credits
- **Voucher**: Accounting document (invoice, payment, journal entry)
- **Ledger**: Account-wise transaction history

---

## Support

For issues or questions about this schema:
1. Check troubleshooting section
2. Review sample queries
3. Consult PostgreSQL documentation
4. Contact development team

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-20  
**PostgreSQL Version**: 12+  
**Schema Version**: 2025_11_20
