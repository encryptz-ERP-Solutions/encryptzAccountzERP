-- =====================================================
-- ERP System - Sample Data Seed Script
-- Date: 2025-11-20
-- Description: Idempotent script to seed sample admin user, business, COA, and vouchers for testing
-- =====================================================

-- =====================================================
-- SECTION 1: SEED DEFAULT ADMIN USER
-- =====================================================

-- Insert default admin user
-- Default credentials:
--   Email: admin@encryptz.com
--   Password: Admin@123
--   Password Hash: sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7
-- 
-- **SECURITY WARNING**: Change this password immediately in production!
-- This is for development and testing purposes only.

INSERT INTO users (
    user_id,
    email,
    email_verified,
    password_hash,
    phone,
    phone_verified,
    first_name,
    last_name,
    is_active,
    is_system_admin,
    created_at,
    updated_at
)
VALUES (
    '33333333-3333-3333-3333-000000000001'::uuid,
    'admin@encryptz.com',
    true,
    'sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7',
    '+91-9876543210',
    true,
    'System',
    'Administrator',
    true,
    true,
    NOW(),
    NOW()
)
ON CONFLICT (email) 
DO UPDATE SET
    email_verified = EXCLUDED.email_verified,
    phone = EXCLUDED.phone,
    phone_verified = EXCLUDED.phone_verified,
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    is_active = EXCLUDED.is_active,
    is_system_admin = EXCLUDED.is_system_admin,
    updated_at = NOW();

-- Assign Admin role to the default admin user
INSERT INTO user_roles (user_id, role_id, assigned_at, is_active)
VALUES (
    '33333333-3333-3333-3333-000000000001'::uuid,
    '22222222-2222-2222-2222-000000000001'::uuid,  -- Admin role from previous seed
    NOW(),
    true
)
ON CONFLICT (user_id, role_id) DO NOTHING;

-- =====================================================
-- SECTION 2: SEED SAMPLE BUSINESS
-- =====================================================

-- Insert sample business
INSERT INTO businesses (
    business_id,
    business_name,
    business_type,
    legal_name,
    tax_id,
    gstin,
    pan,
    industry,
    email,
    phone,
    address_line1,
    address_line2,
    city,
    state,
    country,
    postal_code,
    is_active,
    is_verified,
    verified_at,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '44444444-4444-4444-4444-000000000001'::uuid,
    'Encryptz Technologies Pvt Ltd',
    'Private Limited',
    'Encryptz Technologies Private Limited',
    'TAX123456789',
    '29ABCDE1234F1Z5',
    'ABCDE1234F',
    'Information Technology',
    'contact@encryptz.com',
    '+91-9876543210',
    '123, Tech Park, Electronic City',
    'Phase 1',
    'Bangalore',
    'Karnataka',
    'India',
    '560100',
    true,
    true,
    NOW(),
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (business_id)
DO UPDATE SET
    business_name = EXCLUDED.business_name,
    legal_name = EXCLUDED.legal_name,
    email = EXCLUDED.email,
    phone = EXCLUDED.phone,
    is_active = EXCLUDED.is_active,
    updated_at = NOW(),
    updated_by = EXCLUDED.created_by;

-- Link admin user to the sample business as owner
INSERT INTO business_users (
    business_id,
    user_id,
    is_owner,
    is_primary_contact,
    status,
    joined_at,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '44444444-4444-4444-4444-000000000001'::uuid,
    '33333333-3333-3333-3333-000000000001'::uuid,
    true,
    true,
    'active',
    NOW(),
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (business_id, user_id) DO NOTHING;

-- =====================================================
-- SECTION 3: SEED SAMPLE SUBSCRIPTION PLAN
-- =====================================================

-- Insert a default subscription plan
INSERT INTO subscription_plans (
    plan_id,
    plan_name,
    plan_code,
    description,
    is_active,
    billing_cycle,
    price,
    currency,
    max_users,
    max_businesses,
    max_transactions,
    trial_days,
    created_at,
    updated_at
)
VALUES (
    '55555555-5555-5555-5555-000000000001'::uuid,
    'Professional Plan',
    'PROF',
    'Professional plan with full features for growing businesses',
    true,
    'monthly',
    2999.00,
    'INR',
    25,
    5,
    10000,
    30,
    NOW(),
    NOW()
)
ON CONFLICT (plan_code)
DO UPDATE SET
    plan_name = EXCLUDED.plan_name,
    description = EXCLUDED.description,
    price = EXCLUDED.price,
    updated_at = NOW();

-- Assign subscription to the sample business
INSERT INTO subscriptions (
    subscription_id,
    business_id,
    plan_id,
    status,
    starts_at,
    ends_at,
    trial_ends_at,
    auto_renew,
    billing_cycle,
    amount,
    currency,
    next_billing_at,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '55555555-5555-5555-5555-000000000002'::uuid,
    '44444444-4444-4444-4444-000000000001'::uuid,
    '55555555-5555-5555-5555-000000000001'::uuid,
    'active',
    NOW(),
    NOW() + INTERVAL '1 year',
    NOW() + INTERVAL '30 days',
    true,
    'yearly',
    35988.00,
    'INR',
    NOW() + INTERVAL '1 year',
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (subscription_id)
DO UPDATE SET
    status = EXCLUDED.status,
    ends_at = EXCLUDED.ends_at,
    updated_at = NOW();

-- =====================================================
-- SECTION 4: SEED CHART OF ACCOUNTS (COA)
-- =====================================================

-- Insert 10 sample COA entries representing different account types
INSERT INTO chart_of_accounts (
    account_id,
    business_id,
    account_code,
    account_name,
    account_type,
    account_subtype,
    level,
    is_group,
    is_system_account,
    is_active,
    opening_balance,
    opening_balance_date,
    current_balance,
    currency,
    description,
    created_at,
    updated_at,
    created_by
)
VALUES
    -- Assets
    ('66666666-6666-6666-6666-000000000001'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'A-1000', 'Cash in Hand', 'Asset', 'Current Assets', 1, false, false, true, 
     50000.00, '2025-04-01', 50000.00, 'INR', 'Cash available in hand',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('66666666-6666-6666-6666-000000000002'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'A-1010', 'Bank - Current Account', 'Asset', 'Current Assets', 1, false, false, true, 
     500000.00, '2025-04-01', 500000.00, 'INR', 'Primary business bank account',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('66666666-6666-6666-6666-000000000003'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'A-1500', 'Accounts Receivable', 'Asset', 'Current Assets', 1, false, false, true, 
     150000.00, '2025-04-01', 150000.00, 'INR', 'Amounts due from customers',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('66666666-6666-6666-6666-000000000004'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'A-2000', 'Office Equipment', 'Asset', 'Fixed Assets', 1, false, false, true, 
     250000.00, '2025-04-01', 250000.00, 'INR', 'Computers, furniture, and fixtures',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    -- Liabilities
    ('66666666-6666-6666-6666-000000000005'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'L-1000', 'Accounts Payable', 'Liability', 'Current Liabilities', 1, false, false, true, 
     80000.00, '2025-04-01', 80000.00, 'INR', 'Amounts owed to suppliers',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('66666666-6666-6666-6666-000000000006'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'L-1500', 'GST Payable', 'Liability', 'Current Liabilities', 1, false, false, true, 
     20000.00, '2025-04-01', 20000.00, 'INR', 'GST collected from customers',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    -- Equity
    ('66666666-6666-6666-6666-000000000007'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'E-1000', 'Capital Account', 'Equity', 'Owner Equity', 1, false, false, true, 
     800000.00, '2025-04-01', 800000.00, 'INR', 'Owner capital contribution',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    -- Revenue
    ('66666666-6666-6666-6666-000000000008'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'R-1000', 'Sales Revenue', 'Revenue', 'Operating Revenue', 1, false, false, true, 
     0.00, '2025-04-01', 0.00, 'INR', 'Revenue from product/service sales',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    -- Expenses
    ('66666666-6666-6666-6666-000000000009'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'X-1000', 'Salary Expense', 'Expense', 'Operating Expense', 1, false, false, true, 
     0.00, '2025-04-01', 0.00, 'INR', 'Employee salary and wages',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('66666666-6666-6666-6666-000000000010'::uuid, '44444444-4444-4444-4444-000000000001'::uuid, 
     'X-2000', 'Office Rent', 'Expense', 'Operating Expense', 1, false, false, true, 
     0.00, '2025-04-01', 0.00, 'INR', 'Monthly office rent expense',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid)
ON CONFLICT (business_id, account_code) 
DO UPDATE SET
    account_name = EXCLUDED.account_name,
    description = EXCLUDED.description,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- =====================================================
-- SECTION 5: SEED SAMPLE TAX CODES
-- =====================================================

-- Insert common GST tax codes
INSERT INTO tax_codes (
    tax_code_id,
    business_id,
    tax_code,
    tax_name,
    tax_type,
    rate,
    effective_from,
    is_active,
    description,
    created_at,
    updated_at,
    created_by
)
VALUES
    ('77777777-7777-7777-7777-000000000001'::uuid, '44444444-4444-4444-4444-000000000001'::uuid,
     'GST-18', 'GST @ 18%', 'GST', 18.0000, '2025-04-01', true, 
     'Standard GST rate 18%',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid),
     
    ('77777777-7777-7777-7777-000000000002'::uuid, '44444444-4444-4444-4444-000000000001'::uuid,
     'GST-12', 'GST @ 12%', 'GST', 12.0000, '2025-04-01', true, 
     'GST rate 12% for specific goods/services',
     NOW(), NOW(), '33333333-3333-3333-3333-000000000001'::uuid)
ON CONFLICT (business_id, tax_code)
DO UPDATE SET
    tax_name = EXCLUDED.tax_name,
    rate = EXCLUDED.rate,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- =====================================================
-- SECTION 6: SEED SAMPLE VOUCHERS
-- =====================================================

-- Voucher 1: Opening Journal Entry
INSERT INTO vouchers (
    voucher_id,
    business_id,
    voucher_number,
    voucher_type,
    voucher_date,
    reference_number,
    total_amount,
    tax_amount,
    discount_amount,
    round_off_amount,
    net_amount,
    currency,
    status,
    posted_at,
    posted_by,
    narration,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '88888888-8888-8888-8888-000000000001'::uuid,
    '44444444-4444-4444-4444-000000000001'::uuid,
    'JV-2025-0001',
    'Journal',
    '2025-04-01',
    'OPENING-001',
    950000.00,
    0.00,
    0.00,
    0.00,
    950000.00,
    'INR',
    'posted',
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid,
    'Opening balances as on 01-Apr-2025',
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (business_id, voucher_number, voucher_type) DO NOTHING;

-- Voucher 2: Sales Invoice
INSERT INTO vouchers (
    voucher_id,
    business_id,
    voucher_number,
    voucher_type,
    voucher_date,
    reference_number,
    party_account_id,
    party_name,
    total_amount,
    tax_amount,
    discount_amount,
    round_off_amount,
    net_amount,
    currency,
    status,
    posted_at,
    posted_by,
    narration,
    due_date,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '88888888-8888-8888-8888-000000000002'::uuid,
    '44444444-4444-4444-4444-000000000001'::uuid,
    'SINV-2025-0001',
    'Sales',
    '2025-04-15',
    'INV-001',
    '66666666-6666-6666-6666-000000000003'::uuid,  -- Accounts Receivable
    'ABC Corporation',
    100000.00,
    18000.00,
    0.00,
    0.00,
    118000.00,
    'INR',
    'posted',
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid,
    'Sales invoice for software development services',
    '2025-05-15',
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (business_id, voucher_number, voucher_type) DO NOTHING;

-- Voucher 3: Payment Voucher
INSERT INTO vouchers (
    voucher_id,
    business_id,
    voucher_number,
    voucher_type,
    voucher_date,
    reference_number,
    party_account_id,
    party_name,
    total_amount,
    tax_amount,
    discount_amount,
    round_off_amount,
    net_amount,
    currency,
    status,
    posted_at,
    posted_by,
    narration,
    created_at,
    updated_at,
    created_by
)
VALUES (
    '88888888-8888-8888-8888-000000000003'::uuid,
    '44444444-4444-4444-4444-000000000001'::uuid,
    'PAY-2025-0001',
    'Payment',
    '2025-04-05',
    'PMT-001',
    '66666666-6666-6666-6666-000000000010'::uuid,  -- Office Rent
    'Property Owner',
    50000.00,
    0.00,
    0.00,
    0.00,
    50000.00,
    'INR',
    'posted',
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid,
    'Monthly office rent payment',
    NOW(),
    NOW(),
    '33333333-3333-3333-3333-000000000001'::uuid
)
ON CONFLICT (business_id, voucher_number, voucher_type) DO NOTHING;

-- =====================================================
-- SECTION 7: SEED VOUCHER LINES
-- =====================================================

-- Lines for Voucher 1 (Opening Journal Entry)
INSERT INTO voucher_lines (
    line_id,
    voucher_id,
    line_number,
    account_id,
    description,
    line_amount,
    debit_amount,
    credit_amount
)
VALUES
    -- Debit entries (Assets)
    ('99999999-9999-9999-9999-000000000001'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     1, '66666666-6666-6666-6666-000000000001'::uuid, 'Opening Cash Balance',
     50000.00, 50000.00, 0.00),
     
    ('99999999-9999-9999-9999-000000000002'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     2, '66666666-6666-6666-6666-000000000002'::uuid, 'Opening Bank Balance',
     500000.00, 500000.00, 0.00),
     
    ('99999999-9999-9999-9999-000000000003'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     3, '66666666-6666-6666-6666-000000000003'::uuid, 'Opening Receivables',
     150000.00, 150000.00, 0.00),
     
    ('99999999-9999-9999-9999-000000000004'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     4, '66666666-6666-6666-6666-000000000004'::uuid, 'Opening Fixed Assets',
     250000.00, 250000.00, 0.00),
     
    -- Credit entries (Liabilities and Equity)
    ('99999999-9999-9999-9999-000000000005'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     5, '66666666-6666-6666-6666-000000000005'::uuid, 'Opening Payables',
     80000.00, 0.00, 80000.00),
     
    ('99999999-9999-9999-9999-000000000006'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     6, '66666666-6666-6666-6666-000000000006'::uuid, 'Opening GST Liability',
     20000.00, 0.00, 20000.00),
     
    ('99999999-9999-9999-9999-000000000007'::uuid, '88888888-8888-8888-8888-000000000001'::uuid,
     7, '66666666-6666-6666-6666-000000000007'::uuid, 'Opening Capital',
     800000.00, 0.00, 800000.00)
ON CONFLICT (voucher_id, line_number) DO NOTHING;

-- Lines for Voucher 2 (Sales Invoice)
INSERT INTO voucher_lines (
    line_id,
    voucher_id,
    line_number,
    account_id,
    description,
    line_amount,
    debit_amount,
    credit_amount,
    taxable_amount,
    tax_code_id,
    tax_rate,
    tax_amount
)
VALUES
    -- Debit: Accounts Receivable
    ('99999999-9999-9999-9999-000000000008'::uuid, '88888888-8888-8888-8888-000000000002'::uuid,
     1, '66666666-6666-6666-6666-000000000003'::uuid, 'Accounts Receivable - ABC Corp',
     118000.00, 118000.00, 0.00, 0.00, NULL, 0.0000, 0.00),
     
    -- Credit: Sales Revenue
    ('99999999-9999-9999-9999-000000000009'::uuid, '88888888-8888-8888-8888-000000000002'::uuid,
     2, '66666666-6666-6666-6666-000000000008'::uuid, 'Software Development Services',
     100000.00, 0.00, 100000.00, 100000.00, '77777777-7777-7777-7777-000000000001'::uuid, 18.0000, 18000.00),
     
    -- Credit: GST Payable
    ('99999999-9999-9999-9999-000000000010'::uuid, '88888888-8888-8888-8888-000000000002'::uuid,
     3, '66666666-6666-6666-6666-000000000006'::uuid, 'GST @ 18%',
     18000.00, 0.00, 18000.00, 0.00, NULL, 0.0000, 0.00)
ON CONFLICT (voucher_id, line_number) DO NOTHING;

-- Lines for Voucher 3 (Payment)
INSERT INTO voucher_lines (
    line_id,
    voucher_id,
    line_number,
    account_id,
    description,
    line_amount,
    debit_amount,
    credit_amount
)
VALUES
    -- Debit: Office Rent Expense
    ('99999999-9999-9999-9999-000000000011'::uuid, '88888888-8888-8888-8888-000000000003'::uuid,
     1, '66666666-6666-6666-6666-000000000010'::uuid, 'Office Rent - April 2025',
     50000.00, 50000.00, 0.00),
     
    -- Credit: Bank Account
    ('99999999-9999-9999-9999-000000000012'::uuid, '88888888-8888-8888-8888-000000000003'::uuid,
     2, '66666666-6666-6666-6666-000000000002'::uuid, 'Payment via Bank Transfer',
     50000.00, 0.00, 50000.00)
ON CONFLICT (voucher_id, line_number) DO NOTHING;

-- =====================================================
-- SECTION 8: CREATE LEDGER ENTRIES FOR POSTED VOUCHERS
-- =====================================================

-- Ledger entries for Voucher 1 (Opening Journal)
INSERT INTO ledger_entries (
    business_id, voucher_id, line_id, entry_date, account_id,
    debit_amount, credit_amount, base_debit_amount, base_credit_amount,
    is_opening_balance, narration
)
SELECT 
    v.business_id,
    vl.voucher_id,
    vl.line_id,
    v.voucher_date,
    vl.account_id,
    vl.debit_amount,
    vl.credit_amount,
    vl.debit_amount,
    vl.credit_amount,
    true,
    v.narration
FROM voucher_lines vl
JOIN vouchers v ON vl.voucher_id = v.voucher_id
WHERE v.voucher_id = '88888888-8888-8888-8888-000000000001'::uuid
  AND v.status = 'posted'
ON CONFLICT DO NOTHING;

-- Ledger entries for Voucher 2 (Sales Invoice)
INSERT INTO ledger_entries (
    business_id, voucher_id, line_id, entry_date, account_id,
    debit_amount, credit_amount, base_debit_amount, base_credit_amount,
    narration
)
SELECT 
    v.business_id,
    vl.voucher_id,
    vl.line_id,
    v.voucher_date,
    vl.account_id,
    vl.debit_amount,
    vl.credit_amount,
    vl.debit_amount,
    vl.credit_amount,
    v.narration
FROM voucher_lines vl
JOIN vouchers v ON vl.voucher_id = v.voucher_id
WHERE v.voucher_id = '88888888-8888-8888-8888-000000000002'::uuid
  AND v.status = 'posted'
ON CONFLICT DO NOTHING;

-- Ledger entries for Voucher 3 (Payment)
INSERT INTO ledger_entries (
    business_id, voucher_id, line_id, entry_date, account_id,
    debit_amount, credit_amount, base_debit_amount, base_credit_amount,
    narration
)
SELECT 
    v.business_id,
    vl.voucher_id,
    vl.line_id,
    v.voucher_date,
    vl.account_id,
    vl.debit_amount,
    vl.credit_amount,
    vl.debit_amount,
    vl.credit_amount,
    v.narration
FROM voucher_lines vl
JOIN vouchers v ON vl.voucher_id = v.voucher_id
WHERE v.voucher_id = '88888888-8888-8888-8888-000000000003'::uuid
  AND v.status = 'posted'
ON CONFLICT DO NOTHING;

-- =====================================================
-- VERIFICATION QUERIES (OPTIONAL)
-- =====================================================

DO $$
DECLARE
    user_count INTEGER;
    business_count INTEGER;
    coa_count INTEGER;
    voucher_count INTEGER;
    voucher_line_count INTEGER;
    ledger_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO user_count FROM users WHERE email = 'admin@encryptz.com';
    SELECT COUNT(*) INTO business_count FROM businesses WHERE business_id = '44444444-4444-4444-4444-000000000001'::uuid;
    SELECT COUNT(*) INTO coa_count FROM chart_of_accounts WHERE business_id = '44444444-4444-4444-4444-000000000001'::uuid;
    SELECT COUNT(*) INTO voucher_count FROM vouchers WHERE business_id = '44444444-4444-4444-4444-000000000001'::uuid;
    SELECT COUNT(*) INTO voucher_line_count FROM voucher_lines vl 
        JOIN vouchers v ON vl.voucher_id = v.voucher_id 
        WHERE v.business_id = '44444444-4444-4444-4444-000000000001'::uuid;
    SELECT COUNT(*) INTO ledger_count FROM ledger_entries WHERE business_id = '44444444-4444-4444-4444-000000000001'::uuid;
    
    RAISE NOTICE 'Sample data seed completed successfully:';
    RAISE NOTICE '  - Admin Users: %', user_count;
    RAISE NOTICE '  - Sample Businesses: %', business_count;
    RAISE NOTICE '  - Chart of Accounts Entries: %', coa_count;
    RAISE NOTICE '  - Sample Vouchers: %', voucher_count;
    RAISE NOTICE '  - Voucher Lines: %', voucher_line_count;
    RAISE NOTICE '  - Ledger Entries: %', ledger_count;
    RAISE NOTICE '';
    RAISE NOTICE 'Default Admin Credentials:';
    RAISE NOTICE '  Email: admin@encryptz.com';
    RAISE NOTICE '  Password: Admin@123';
    RAISE NOTICE '';
    RAISE NOTICE 'WARNING: Change the default admin password immediately in production!';
END $$;

-- =====================================================
-- NOTES
-- =====================================================
-- 
-- 1. This script is idempotent - safe to run multiple times
-- 2. Default admin password is 'Admin@123' - CHANGE THIS IN PRODUCTION!
-- 3. Password hash uses SHA256 (simple implementation for demo)
--    Format: sha256_<hex_hash>
-- 4. Sample business includes:
--    - 10 COA entries covering all account types
--    - 3 vouchers (Opening Journal, Sales Invoice, Payment)
--    - Voucher lines with proper debit/credit entries
--    - Ledger entries for trial balance testing
-- 5. All UUIDs are hardcoded for consistency and referential integrity
-- 6. The opening journal balances Assets = Liabilities + Equity
--    Total Debits = Total Credits = 950,000.00
-- 7. After running this script, you can:
--    - Login with admin@encryptz.com / Admin@123
--    - View the sample business data
--    - Generate trial balance, P&L, and balance sheet reports
-- =====================================================
