/*
================================================================================================
Filename: verify_chart_of_accounts.sql
Description: Diagnostic script to verify Chart of Accounts data
================================================================================================
*/

-- 1. Check all businesses
SELECT 
    business_id,
    business_name,
    business_code,
    is_active
FROM core.businesses
ORDER BY created_at_utc;

-- 2. Check account types
SELECT 
    account_type_id,
    account_type_name,
    normal_balance
FROM acct.account_types
ORDER BY account_type_id;

-- 3. Count accounts per business
SELECT 
    b.business_id,
    b.business_name,
    COUNT(coa.account_id) as account_count
FROM core.businesses b
LEFT JOIN acct.chart_of_accounts coa ON b.business_id = coa.business_id
GROUP BY b.business_id, b.business_name
ORDER BY account_count DESC;

-- 4. List all accounts for a specific business (replace with your business_id)
-- Replace 'YOUR-BUSINESS-ID' with the actual business_id from step 1
SELECT 
    coa.account_code,
    coa.account_name,
    at.account_type_name,
    parent.account_name as parent_account_name,
    coa.is_system_account,
    coa.is_active,
    coa.business_id
FROM acct.chart_of_accounts coa
LEFT JOIN acct.account_types at ON coa.account_type_id = at.account_type_id
LEFT JOIN acct.chart_of_accounts parent ON coa.parent_account_id = parent.account_id
WHERE coa.business_id = '44444444-4444-4444-4444-aaaaaaaaaaaa'  -- Replace this!
ORDER BY coa.account_code;

-- 5. List all accounts (to see which business they belong to)
SELECT 
    b.business_name,
    coa.account_code,
    coa.account_name,
    at.account_type_name,
    coa.is_system_account,
    coa.is_active
FROM acct.chart_of_accounts coa
LEFT JOIN core.businesses b ON coa.business_id = b.business_id
LEFT JOIN acct.account_types at ON coa.account_type_id = at.account_type_id
ORDER BY b.business_name, coa.account_code;

-- 6. Check for accounts with missing account types
SELECT 
    coa.account_id,
    coa.account_code,
    coa.account_name,
    coa.account_type_id,
    coa.business_id
FROM acct.chart_of_accounts coa
WHERE NOT EXISTS (
    SELECT 1 FROM acct.account_types at 
    WHERE at.account_type_id = coa.account_type_id
);

-- 7. Get the business_id that was used in the seed script
-- This shows the first business (which the seed script uses by default)
SELECT 
    business_id,
    business_name,
    business_code
FROM core.businesses
ORDER BY created_at_utc
LIMIT 1;

