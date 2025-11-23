/*
================================================================================================
Filename: 2025_11_23_seed_chart_of_accounts.sql
Description: Seed script for standard Chart of Accounts structure
Version: 1.0
Database: PostgreSQL 14+
================================================================================================

This script creates a standard Chart of Accounts structure with:
- LIABILITIES (Liability accounts)
- ASSETS (Asset accounts)
- INCOME (Revenue accounts)
- EXPENSES (Expense accounts)

Usage:
1. Replace :BUSINESS_ID with your actual business_id UUID
2. Or use a variable: \set business_id 'your-uuid-here'
3. Run the script

Note: Account Type IDs are assumed to be:
  1 = Asset
  2 = Liability
  3 = Equity
  4 = Revenue
  5 = Expense
*/

-- =================================================================================================
-- Helper function to get account type ID by name
-- =================================================================================================
DO $$
DECLARE
    v_asset_type_id INTEGER;
    v_liability_type_id INTEGER;
    v_revenue_type_id INTEGER;
    v_expense_type_id INTEGER;
    v_business_id UUID;
    
    -- Account IDs for parent-child relationships
    v_liabilities_id UUID;
    v_assets_id UUID;
    v_income_id UUID;
    v_expenses_id UUID;
    
    -- Sub-category IDs
    v_branch_divisions_id UUID;
    v_capital_account_id UUID;
    v_reserves_surplus_id UUID;
    v_current_liabilities_id UUID;
    v_duties_taxes_id UUID;
    v_provisions_id UUID;
    v_sundry_creditors_id UUID;
    v_loans_liability_id UUID;
    v_secured_loans_id UUID;
    v_unsecured_loans_id UUID;
    v_suspense_ac_id UUID;
    v_profit_loss_ac_id UUID;
    
    v_current_assets_id UUID;
    v_bank_accounts_id UUID;
    v_bank_ac_id UUID;
    v_cash_in_hand_id UUID;
    v_cash_id UUID;
    v_deposits_asset_id UUID;
    v_loans_advances_asset_id UUID;
    v_inventory_id UUID;
    v_sundry_debtors_id UUID;
    v_ham_id UUID;
    v_ham1_id UUID;
    v_ham2_id UUID;
    v_ham3_id UUID;
    v_fixed_assets_id UUID;
    v_investments_id UUID;
    v_misc_expenses_asset_id UUID;
    
    v_direct_incomes_id UUID;
    v_indirect_incomes_id UUID;
    v_sales_accounts_id UUID;
    
    v_direct_expenses_id UUID;
    v_purchase_accounts_id UUID;
    v_inward_supply_id UUID;
    v_indirect_expenses_id UUID;
    v_suspense_expense_id UUID;
    v_telephone_expense_id UUID;
BEGIN
    -- Get account type IDs
    SELECT account_type_id INTO v_asset_type_id FROM acct.account_types WHERE account_type_name = 'Asset' LIMIT 1;
    SELECT account_type_id INTO v_liability_type_id FROM acct.account_types WHERE account_type_name = 'Liability' LIMIT 1;
    SELECT account_type_id INTO v_revenue_type_id FROM acct.account_types WHERE account_type_name = 'Revenue' LIMIT 1;
    SELECT account_type_id INTO v_expense_type_id FROM acct.account_types WHERE account_type_name = 'Expense' LIMIT 1;
    
    -- Get business_id from parameter or use first business
    -- You can set this via: \set business_id 'your-uuid-here'
    -- For now, we'll use the first business in the system
    SELECT business_id INTO v_business_id FROM core.businesses where businesses.business_id='44444444-4444-4444-4444-aaaaaaaaaaaa' LIMIT 1;
    
    IF v_business_id IS NULL THEN
        RAISE EXCEPTION 'No business found. Please create a business first or set the business_id variable.';
    END IF;
    
    RAISE NOTICE 'Using business_id: %', v_business_id;
    
    -- =================================================================================================
    -- LIABILITIES
    -- =================================================================================================
    
    -- Main LIABILITIES category
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name, 
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, NULL, '2000', 'LIABILITIES',
        'Liabilities - All obligations and debts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_liabilities_id;
    
    -- Branch / Divisions
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2100', 'Branch / Divisions',
        'Branch and division accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_branch_divisions_id;
    
    -- Capital Account
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2200', 'Capital Account',
        'Capital and equity accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_capital_account_id;
    
    -- Reserves & Surplus
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2300', 'Reserves & Surplus',
        'Reserves and surplus accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_reserves_surplus_id;
    
    -- Current Liabilities
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2400', 'Current Liabilities',
        'Current liabilities and short-term obligations', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_current_liabilities_id;
    
    -- Duties & Taxes
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_current_liabilities_id, '2410', 'Duties & Taxes',
        'Duties and taxes payable', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_duties_taxes_id;
    
    -- CGST
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_duties_taxes_id, '2411', 'CGST',
        'Central Goods and Services Tax', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    );
    
    -- SGST / UTGST
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_duties_taxes_id, '2412', 'SGST / UTGST',
        'State Goods and Services Tax / Union Territory GST', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    );
    
    -- IGST
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_duties_taxes_id, '2413', 'IGST',
        'Integrated Goods and Services Tax', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    );
    
    -- Provisions
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_current_liabilities_id, '2420', 'Provisions',
        'Provisions for liabilities', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_provisions_id;
    
    -- Sundry Creditors
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_current_liabilities_id, '2430', 'Sundry Creditors',
        'Sundry creditors and trade payables', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_sundry_creditors_id;
    
    -- Loans (Liability)
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2500', 'Loans (Liability)',
        'Loans and borrowings', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_loans_liability_id;
    
    -- Secured Loans
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_loans_liability_id, '2510', 'Secured Loans',
        'Secured loans and borrowings', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_secured_loans_id;
    
    -- Unsecured Loans
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_loans_liability_id, '2520', 'Unsecured Loans',
        'Unsecured loans and borrowings', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_unsecured_loans_id;
    
    -- Suspense A/c
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2600', 'Suspense A/c',
        'Suspense account for temporary entries', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_suspense_ac_id;
    
    -- Profit & Loss A/c
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_liability_type_id, v_liabilities_id, '2700', 'Profit & Loss A/c',
        'Profit and Loss account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_profit_loss_ac_id;
    
    -- =================================================================================================
    -- ASSETS
    -- =================================================================================================
    
    -- Main ASSETS category
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, NULL, '1000', 'ASSETS',
        'Assets - All resources and properties', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_assets_id;
    
    -- Current Assets
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_assets_id, '1100', 'Current Assets',
        'Current assets and short-term resources', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_current_assets_id;
    
    -- Bank Accounts
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1110', 'Bank Accounts',
        'Bank account group', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_bank_accounts_id;
    
    -- Bank A/c
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_bank_accounts_id, '1111', 'Bank A/c',
        'Main bank account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_bank_ac_id;
    
    -- Cash-in-Hand
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1120', 'Cash-in-Hand',
        'Cash in hand group', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_cash_in_hand_id;
    
    -- Cash
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_cash_in_hand_id, '1121', 'Cash',
        'Cash account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_cash_id;
    
    -- Deposits (Asset)
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1130', 'Deposits (Asset)',
        'Deposits and advances given', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_deposits_asset_id;
    
    -- Loans & Advances (Asset)
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1140', 'Loans & Advances (Asset)',
        'Loans and advances given', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_loans_advances_asset_id;
    
    -- Inventory
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1150', 'Inventory',
        'Inventory and stock', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_inventory_id;
    
    -- Sundry Debtors
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_current_assets_id, '1160', 'Sundry Debtors',
        'Sundry debtors and trade receivables', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_sundry_debtors_id;
    
    -- Ham
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_sundry_debtors_id, '1161', 'Ham',
        'Ham debtor account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_ham_id;
    
    -- Ham 1
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_sundry_debtors_id, '1162', 'Ham 1',
        'Ham 1 debtor account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_ham1_id;
    
    -- Ham 2
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_sundry_debtors_id, '1163', 'Ham 2',
        'Ham 2 debtor account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_ham2_id;
    
    -- Ham 3
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_sundry_debtors_id, '1164', 'Ham 3',
        'Ham 3 debtor account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_ham3_id;
    
    -- Fixed Assets
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_assets_id, '1200', 'Fixed Assets',
        'Fixed assets and long-term resources', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_fixed_assets_id;
    
    -- Investments
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_assets_id, '1300', 'Investments',
        'Investments and securities', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_investments_id;
    
    -- Misc. Expenses (ASSET)
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_asset_type_id, v_assets_id, '1400', 'Misc. Expenses (ASSET)',
        'Miscellaneous expenses treated as assets', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_misc_expenses_asset_id;
    
    -- =================================================================================================
    -- INCOME
    -- =================================================================================================
    
    -- Main INCOME category
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_revenue_type_id, NULL, '4000', 'INCOME',
        'Income - All revenue and income sources', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_income_id;
    
    -- Direct Incomes
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_revenue_type_id, v_income_id, '4100', 'Direct Incomes',
        'Direct income accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_direct_incomes_id;
    
    -- Indirect Incomes
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_revenue_type_id, v_income_id, '4200', 'Indirect Incomes',
        'Indirect income accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_indirect_incomes_id;
    
    -- Sales Accounts
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_revenue_type_id, v_income_id, '4300', 'Sales Accounts',
        'Sales and revenue accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_sales_accounts_id;
    
    -- =================================================================================================
    -- EXPENSES
    -- =================================================================================================
    
    -- Main EXPENSES category
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, NULL, '5000', 'EXPENSES',
        'Expenses - All costs and expenditures', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_expenses_id;
    
    -- Direct Expenses
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_expenses_id, '5100', 'Direct Expenses',
        'Direct expense accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_direct_expenses_id;
    
    -- Purchase Accounts
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_direct_expenses_id, '5110', 'Purchase Accounts',
        'Purchase and procurement accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_purchase_accounts_id;
    
    -- Inward Supply
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_direct_expenses_id, '5120', 'Inward Supply',
        'Inward supply and procurement', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_inward_supply_id;
    
    -- Indirect Expenses
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_expenses_id, '5200', 'Indirect Expenses',
        'Indirect expense accounts', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_indirect_expenses_id;
    
    -- Suspense
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_indirect_expenses_id, '5210', 'Suspense',
        'Suspense expense account', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_suspense_expense_id;
    
    -- Telephone Expense _ 18%
    INSERT INTO acct.chart_of_accounts (
        account_id, business_id, account_type_id, parent_account_id, account_code, account_name,
        description, is_active, is_system_account, created_at_utc
    ) VALUES (
        gen_random_uuid(), v_business_id, v_expense_type_id, v_indirect_expenses_id, '5220', 'Telephone Expense _ 18%',
        'Telephone expense with 18% GST', TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
    ) RETURNING account_id INTO v_telephone_expense_id;
    
    RAISE NOTICE 'Chart of Accounts seeded successfully for business_id: %', v_business_id;
    
END $$;

-- =================================================================================================
-- Verification Query
-- =================================================================================================
-- Run this query to verify the accounts were created:
-- SELECT 
--     coa.account_code,
--     coa.account_name,
--     at.account_type_name,
--     parent.account_name as parent_account_name,
--     coa.is_system_account,
--     coa.is_active
-- FROM acct.chart_of_accounts coa
-- LEFT JOIN acct.account_types at ON coa.account_type_id = at.account_type_id
-- LEFT JOIN acct.chart_of_accounts parent ON coa.parent_account_id = parent.account_id
-- ORDER BY coa.account_code;

