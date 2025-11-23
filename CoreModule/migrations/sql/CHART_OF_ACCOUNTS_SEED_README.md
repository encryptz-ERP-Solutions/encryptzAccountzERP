# Chart of Accounts Seed Script

## Overview
This script seeds a standard Chart of Accounts structure with the following main categories:
- **LIABILITIES** (2000 series)
- **ASSETS** (1000 series)
- **INCOME** (4000 series)
- **EXPENSES** (5000 series)

## Prerequisites
1. PostgreSQL database with the `acct` schema
2. Account types must be seeded (Asset, Liability, Revenue, Expense)
3. At least one business must exist in `core.businesses`

## Usage

### Option 1: Use First Business (Default)
The script will automatically use the first business in the system:
```sql
\i migrations/sql/2025_11_23_seed_chart_of_accounts.sql
```

### Option 2: Specify Business ID
If you want to seed accounts for a specific business, modify the script:

1. Find your business_id:
```sql
SELECT business_id, business_name, business_code 
FROM core.businesses;
```

2. Edit the script and replace this line:
```sql
SELECT business_id INTO v_business_id FROM core.businesses LIMIT 1;
```

With:
```sql
SELECT business_id INTO v_business_id FROM core.businesses WHERE business_id = 'YOUR-BUSINESS-UUID-HERE';
```

Or use a variable in psql:
```sql
\set business_id 'your-uuid-here'
-- Then modify the script to use :business_id
```

## Account Structure

### LIABILITIES (2000)
- Branch / Divisions (2100)
- Capital Account (2200)
- Reserves & Surplus (2300)
- Current Liabilities (2400)
  - Duties & Taxes (2410)
    - CGST (2411)
    - SGST / UTGST (2412)
    - IGST (2413)
  - Provisions (2420)
  - Sundry Creditors (2430)
- Loans (Liability) (2500)
  - Secured Loans (2510)
  - Unsecured Loans (2520)
- Suspense A/c (2600)
- Profit & Loss A/c (2700)

### ASSETS (1000)
- Current Assets (1100)
  - Bank Accounts (1110)
    - Bank A/c (1111)
  - Cash-in-Hand (1120)
    - Cash (1121)
  - Deposits (Asset) (1130)
  - Loans & Advances (Asset) (1140)
  - Inventory (1150)
  - Sundry Debtors (1160)
    - Ham (1161)
    - Ham 1 (1162)
    - Ham 2 (1163)
    - Ham 3 (1164)
- Fixed Assets (1200)
- Investments (1300)
- Misc. Expenses (ASSET) (1400)

### INCOME (4000)
- Direct Incomes (4100)
- Indirect Incomes (4200)
- Sales Accounts (4300)

### EXPENSES (5000)
- Direct Expenses (5100)
  - Purchase Accounts (5110)
  - Inward Supply (5120)
- Indirect Expenses (5200)
  - Suspense (5210)
  - Telephone Expense _ 18% (5220)

## Verification

After running the script, verify the accounts were created:

```sql
SELECT 
    coa.account_code,
    coa.account_name,
    at.account_type_name,
    parent.account_name as parent_account_name,
    coa.is_system_account,
    coa.is_active
FROM acct.chart_of_accounts coa
LEFT JOIN acct.account_types at ON coa.account_type_id = at.account_type_id
LEFT JOIN acct.chart_of_accounts parent ON coa.parent_account_id = parent.account_id
WHERE coa.business_id = 'YOUR-BUSINESS-ID'
ORDER BY coa.account_code;
```

## Notes
- All accounts are marked as `is_system_account = TRUE` to prevent accidental deletion
- All accounts are marked as `is_active = TRUE` by default
- Account codes follow a hierarchical numbering system
- The script uses PostgreSQL's `gen_random_uuid()` for account IDs
- The script is idempotent-safe (can be run multiple times, but will create duplicates if run again)

## Troubleshooting

### Error: "No business found"
- Ensure at least one business exists in `core.businesses`
- Or modify the script to use a specific business_id

### Error: "Account type not found"
- Ensure account types are seeded:
```sql
SELECT * FROM acct.account_types;
```
- If missing, run the account types seed script first

### Duplicate accounts
- The script will create duplicate accounts if run multiple times
- To avoid duplicates, check if accounts exist before running:
```sql
SELECT COUNT(*) FROM acct.chart_of_accounts WHERE business_id = 'YOUR-BUSINESS-ID';
```

