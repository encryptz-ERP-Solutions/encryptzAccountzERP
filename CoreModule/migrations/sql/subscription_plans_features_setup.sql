BEGIN;

-- Ensure core schema exists
CREATE SCHEMA IF NOT EXISTS core;

-- Drop new feature/addon tables if they exist (these are new tables, safe to drop)
DROP TABLE IF EXISTS core.plan_addons CASCADE;
DROP TABLE IF EXISTS core.plan_features CASCADE;
DROP TABLE IF EXISTS core.addons CASCADE;
DROP TABLE IF EXISTS core.features CASCADE;

-- Add new columns to existing subscription_plans table if they don't exist
-- This preserves existing data and adds the new structure
DO $$
BEGIN
    -- Add subscription_fee column (maps to existing price column)
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_schema = 'core' AND table_name = 'subscription_plans' AND column_name = 'subscription_fee') THEN
        ALTER TABLE core.subscription_plans ADD COLUMN subscription_fee NUMERIC(12,2);
        -- Copy existing price to subscription_fee
        UPDATE core.subscription_plans SET subscription_fee = price WHERE subscription_fee IS NULL;
        ALTER TABLE core.subscription_plans ALTER COLUMN subscription_fee SET NOT NULL;
    END IF;

    -- Add business_ad_free_limit column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_schema = 'core' AND table_name = 'subscription_plans' AND column_name = 'business_ad_free_limit') THEN
        ALTER TABLE core.subscription_plans ADD COLUMN business_ad_free_limit TEXT;
    END IF;

    -- Add company_creation column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_schema = 'core' AND table_name = 'subscription_plans' AND column_name = 'company_creation') THEN
        ALTER TABLE core.subscription_plans ADD COLUMN company_creation TEXT;
    END IF;

    -- Add users_limit column (maps to existing max_users, but as TEXT for 'Unlimited')
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_schema = 'core' AND table_name = 'subscription_plans' AND column_name = 'users_limit') THEN
        ALTER TABLE core.subscription_plans ADD COLUMN users_limit TEXT;
        -- Convert max_users to text format
        UPDATE core.subscription_plans SET users_limit = max_users::TEXT WHERE users_limit IS NULL;
    END IF;

    -- Add auditor_access column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_schema = 'core' AND table_name = 'subscription_plans' AND column_name = 'auditor_access') THEN
        ALTER TABLE core.subscription_plans ADD COLUMN auditor_access TEXT;
    END IF;
END $$;

-- Create addons table
CREATE TABLE IF NOT EXISTS core.addons (
    addon_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    addon_code TEXT UNIQUE NOT NULL,
    addon_name TEXT NOT NULL,
    price NUMERIC(12,2) NOT NULL,
    created_at_utc TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at_utc TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create features table
CREATE TABLE IF NOT EXISTS core.features (
    feature_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    feature_code TEXT UNIQUE NOT NULL,
    feature_name TEXT NOT NULL,
    category TEXT NOT NULL,
    price NUMERIC(12,2) NOT NULL,
    is_free_for_all BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT,
    created_at_utc TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at_utc TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create plan_features table
CREATE TABLE IF NOT EXISTS core.plan_features (
    plan_id BIGINT NOT NULL,
    feature_id BIGINT NOT NULL REFERENCES core.features(feature_id) ON DELETE CASCADE,
    included BOOLEAN NOT NULL,
    addon_price NUMERIC(12,2),
    limit_value TEXT,
    PRIMARY KEY(plan_id, feature_id)
);

-- Create plan_addons table
CREATE TABLE IF NOT EXISTS core.plan_addons (
    plan_id BIGINT NOT NULL,
    addon_id BIGINT NOT NULL REFERENCES core.addons(addon_id) ON DELETE CASCADE,
    available BOOLEAN NOT NULL,
    PRIMARY KEY(plan_id, addon_id)
);

-- Add foreign key constraints for plan_features
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'fk_plan_features_plan_id' 
        AND conrelid = 'core.plan_features'::regclass
    ) THEN
        ALTER TABLE core.plan_features 
        ADD CONSTRAINT fk_plan_features_plan_id 
        FOREIGN KEY (plan_id) REFERENCES core.subscription_plans(plan_id) ON DELETE CASCADE;
    END IF;
END $$;

-- Add foreign key constraints for plan_addons
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'fk_plan_addons_plan_id' 
        AND conrelid = 'core.plan_addons'::regclass
    ) THEN
        ALTER TABLE core.plan_addons 
        ADD CONSTRAINT fk_plan_addons_plan_id 
        FOREIGN KEY (plan_id) REFERENCES core.subscription_plans(plan_id) ON DELETE CASCADE;
    END IF;
END $$;

-- Insert or update subscription plans in the existing table
-- First, ensure plan_name has a unique constraint if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'subscription_plans_plan_name_key' 
        AND conrelid = 'core.subscription_plans'::regclass
    ) THEN
        CREATE UNIQUE INDEX IF NOT EXISTS subscription_plans_plan_name_key ON core.subscription_plans(plan_name);
    END IF;
END $$;

-- Insert or update subscription plans
INSERT INTO core.subscription_plans (plan_name, subscription_fee, business_ad_free_limit, company_creation, users_limit, auditor_access, description, price, max_users, max_businesses, is_publicly_visible, is_active)
VALUES
    ('FREE', 0.00, '0', 'Unlimited', '2', '1', 'Free plan with basic features', 0.00, 2, 1, TRUE, TRUE),
    ('SILVER', 2499.00, '1', 'Unlimited', '2', '2', 'Silver plan with enhanced features', 2499.00, 2, 1, TRUE, TRUE),
    ('GOLD', 3499.00, '5', 'Unlimited', '5', '3', 'Gold plan with premium features', 3499.00, 5, 5, TRUE, TRUE),
    ('PLATINUM', 4999.00, 'Unlimited', 'Unlimited', 'Unlimited', 'Unlimited', 'Platinum plan with all features', 4999.00, 999999, 999999, TRUE, TRUE)
ON CONFLICT (plan_name) DO UPDATE SET
    subscription_fee = EXCLUDED.subscription_fee,
    business_ad_free_limit = EXCLUDED.business_ad_free_limit,
    company_creation = EXCLUDED.company_creation,
    users_limit = EXCLUDED.users_limit,
    auditor_access = EXCLUDED.auditor_access,
    description = COALESCE(EXCLUDED.description, core.subscription_plans.description),
    price = EXCLUDED.price,
    max_users = EXCLUDED.max_users,
    max_businesses = EXCLUDED.max_businesses,
    is_publicly_visible = EXCLUDED.is_publicly_visible,
    is_active = EXCLUDED.is_active;

-- Insert addons
INSERT INTO core.addons (addon_code, addon_name, price)
VALUES
    ('AD_FEE_BUSINESS', 'Ad Fee Business', 99.00),
    ('USER_ADDON', 'User Addon', 10.00)
ON CONFLICT (addon_code) DO UPDATE SET
    addon_name = EXCLUDED.addon_name,
    price = EXCLUDED.price,
    updated_at_utc = CURRENT_TIMESTAMP;

-- Insert features
INSERT INTO core.features (feature_code, feature_name, category, price, is_free_for_all)
VALUES
    -- CUSTOMER MANAGEMENT
    ('LOYALTY_MGMT', 'Loyalty Management', 'CUSTOMER MANAGEMENT', 10.00, FALSE),
    ('PRICE_LEVEL', 'Price Level Settings', 'CUSTOMER MANAGEMENT', 10.00, FALSE),
    ('PARTY_CREDIT_LIMIT', 'Party Credit Limit', 'CUSTOMER MANAGEMENT', 10.00, FALSE),
    -- STOCK & SALE
    ('ITEMS_BULK_UPDATE', 'Items Bulk Update', 'STOCK & SALE', 10.00, FALSE),
    ('IMPORT_EXPORT', 'Import & Export', 'STOCK & SALE', 10.00, FALSE),
    ('ADS_FEE_INVOICE', 'Ads Fee Invoice', 'STOCK & SALE', 10.00, FALSE),
    ('MULTIPLE_GODOWN', 'Multiple Godown', 'STOCK & SALE', 10.00, FALSE),
    ('BARCODE_LABEL', 'Barcode & Label Printing', 'STOCK & SALE', 0.00, TRUE),
    ('AUTO_EWAYBILL', 'Auto Generation of Eway Bill', 'STOCK & SALE', 10.00, FALSE),
    ('MANUFACTURING', 'Manufacturing', 'STOCK & SALE', 10.00, FALSE),
    ('TDS_TCS_INVOICE', 'TDS/TCS on Invoice', 'STOCK & SALE', 10.00, FALSE),
    ('POS_INTEGRATION', 'POS Integration', 'STOCK & SALE', 50.00, FALSE),
    ('E_INVOICING', 'E-Invoicing', 'STOCK & SALE', 50.00, FALSE),
    ('INVENTORY_MERGING', 'Inventory Merging', 'STOCK & SALE', 499.00, FALSE),
    -- TRANSACTIONS
    ('RESTORE_DELETED_TXNS', 'Restore Deleted Txns', 'TRANSACTIONS', 10.00, FALSE),
    ('IMPORT_TALLY', 'Import Data from Tally', 'TRANSACTIONS', 0.00, TRUE),
    ('EXPORT_TALLY', 'Export Data to Tally', 'TRANSACTIONS', 10.00, FALSE),
    -- REPORT
    ('TRIAL_BALANCE', 'Trial Balance', 'REPORT', 0.00, TRUE),
    ('BALANCE_SHEET', 'Balancesheet', 'REPORT', 0.00, TRUE),
    ('PROFIT_LOSS', 'Profit & Loss Statement', 'REPORT', 0.00, TRUE),
    ('BILLWISE_PNL', 'Billwise Profit & Loss', 'REPORT', 10.00, FALSE),
    ('PARTYWISE_PNL', 'Party Wise Profit & Loss', 'REPORT', 10.00, FALSE),
    ('GST_REPORTS', 'GST Reports [GSTR 1 / 2 / 3B]', 'REPORT', 0.00, TRUE),
    -- OTHER UTILITIES
    ('AUTO_BACKUP_GDRIVE', 'Autobackup to Google Drive', 'OTHER UTILITIES', 10.00, FALSE),
    -- ACCOUNTS MODULE
    ('SALES', 'Sales', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('PURCHASE', 'Purchase', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('RECEIPT', 'Receipt', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('PAYMENT', 'Payment', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('CONTRA', 'Contra', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('JOURNAL', 'Journal', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('CREDIT_NOTE', 'Credit Note', 'ACCOUNTS MODULE', 0.00, TRUE),
    ('DEBIT_NOTE', 'Debit Note', 'ACCOUNTS MODULE', 0.00, TRUE)
ON CONFLICT (feature_code) DO UPDATE SET
    feature_name = EXCLUDED.feature_name,
    category = EXCLUDED.category,
    price = EXCLUDED.price,
    is_free_for_all = EXCLUDED.is_free_for_all,
    updated_at_utc = CURRENT_TIMESTAMP;

-- Insert plan_features for all plan-feature combinations
-- First, get all plan and feature IDs into temp variables via CTEs
WITH plan_ids AS (
    SELECT plan_id, plan_name FROM core.subscription_plans
),
feature_ids AS (
    SELECT feature_id, feature_code, price, is_free_for_all FROM core.features
),
plan_feature_combinations AS (
    SELECT 
        p.plan_id,
        f.feature_id,
        CASE
            -- CUSTOMER MANAGEMENT: SILVER, GOLD, PLATINUM only
            WHEN f.feature_code IN ('LOYALTY_MGMT', 'PRICE_LEVEL', 'PARTY_CREDIT_LIMIT') 
                THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
            -- STOCK & SALE: ITEMS_BULK_UPDATE, IMPORT_EXPORT, ADS_FEE_INVOICE, MULTIPLE_GODOWN: SILVER, GOLD, PLATINUM
            WHEN f.feature_code IN ('ITEMS_BULK_UPDATE', 'IMPORT_EXPORT', 'ADS_FEE_INVOICE', 'MULTIPLE_GODOWN')
                THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
            -- BARCODE_LABEL: ALL plans
            WHEN f.feature_code = 'BARCODE_LABEL' THEN TRUE
            -- AUTO_EWAYBILL, MANUFACTURING, TDS_TCS_INVOICE, POS_INTEGRATION: GOLD & PLATINUM only
            WHEN f.feature_code IN ('AUTO_EWAYBILL', 'MANUFACTURING', 'TDS_TCS_INVOICE', 'POS_INTEGRATION')
                THEN p.plan_name IN ('GOLD', 'PLATINUM')
            -- E_INVOICING, INVENTORY_MERGING: PLATINUM only
            WHEN f.feature_code IN ('E_INVOICING', 'INVENTORY_MERGING')
                THEN p.plan_name = 'PLATINUM'
            -- TRANSACTIONS: RESTORE_DELETED_TXNS: GOLD & PLATINUM only
            WHEN f.feature_code = 'RESTORE_DELETED_TXNS'
                THEN p.plan_name IN ('GOLD', 'PLATINUM')
            -- IMPORT_TALLY: ALL plans
            WHEN f.feature_code = 'IMPORT_TALLY' THEN TRUE
            -- EXPORT_TALLY: PLATINUM only
            WHEN f.feature_code = 'EXPORT_TALLY'
                THEN p.plan_name = 'PLATINUM'
            -- REPORT: TRIAL_BALANCE, BALANCE_SHEET, PROFIT_LOSS, GST_REPORTS: ALL plans
            WHEN f.feature_code IN ('TRIAL_BALANCE', 'BALANCE_SHEET', 'PROFIT_LOSS', 'GST_REPORTS')
                THEN TRUE
            -- BILLWISE_PNL, PARTYWISE_PNL: GOLD & PLATINUM only
            WHEN f.feature_code IN ('BILLWISE_PNL', 'PARTYWISE_PNL')
                THEN p.plan_name IN ('GOLD', 'PLATINUM')
            -- OTHER UTILITIES: AUTO_BACKUP_GDRIVE: SILVER, GOLD, PLATINUM
            WHEN f.feature_code = 'AUTO_BACKUP_GDRIVE'
                THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
            -- ACCOUNTS MODULE: ALL plans
            WHEN f.feature_code IN ('SALES', 'PURCHASE', 'RECEIPT', 'PAYMENT', 'CONTRA', 'JOURNAL', 'CREDIT_NOTE', 'DEBIT_NOTE')
                THEN TRUE
            ELSE FALSE
        END AS included,
        CASE
            WHEN f.is_free_for_all THEN 0.00
            WHEN CASE
                WHEN f.feature_code IN ('LOYALTY_MGMT', 'PRICE_LEVEL', 'PARTY_CREDIT_LIMIT') 
                    THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
                WHEN f.feature_code IN ('ITEMS_BULK_UPDATE', 'IMPORT_EXPORT', 'ADS_FEE_INVOICE', 'MULTIPLE_GODOWN')
                    THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
                WHEN f.feature_code = 'BARCODE_LABEL' THEN TRUE
                WHEN f.feature_code IN ('AUTO_EWAYBILL', 'MANUFACTURING', 'TDS_TCS_INVOICE', 'POS_INTEGRATION')
                    THEN p.plan_name IN ('GOLD', 'PLATINUM')
                WHEN f.feature_code IN ('E_INVOICING', 'INVENTORY_MERGING')
                    THEN p.plan_name = 'PLATINUM'
                WHEN f.feature_code = 'RESTORE_DELETED_TXNS'
                    THEN p.plan_name IN ('GOLD', 'PLATINUM')
                WHEN f.feature_code = 'IMPORT_TALLY' THEN TRUE
                WHEN f.feature_code = 'EXPORT_TALLY'
                    THEN p.plan_name = 'PLATINUM'
                WHEN f.feature_code IN ('TRIAL_BALANCE', 'BALANCE_SHEET', 'PROFIT_LOSS', 'GST_REPORTS')
                    THEN TRUE
                WHEN f.feature_code IN ('BILLWISE_PNL', 'PARTYWISE_PNL')
                    THEN p.plan_name IN ('GOLD', 'PLATINUM')
                WHEN f.feature_code = 'AUTO_BACKUP_GDRIVE'
                    THEN p.plan_name IN ('SILVER', 'GOLD', 'PLATINUM')
                WHEN f.feature_code IN ('SALES', 'PURCHASE', 'RECEIPT', 'PAYMENT', 'CONTRA', 'JOURNAL', 'CREDIT_NOTE', 'DEBIT_NOTE')
                    THEN TRUE
                ELSE FALSE
            END THEN f.price
            ELSE NULL
        END AS addon_price
    FROM plan_ids p
    CROSS JOIN feature_ids f
)
INSERT INTO core.plan_features (plan_id, feature_id, included, addon_price)
SELECT plan_id, feature_id, included, addon_price
FROM plan_feature_combinations
ON CONFLICT (plan_id, feature_id) DO UPDATE SET
    included = EXCLUDED.included,
    addon_price = EXCLUDED.addon_price;

-- Insert plan_addons
WITH plan_ids AS (
    SELECT plan_id, plan_name FROM core.subscription_plans
),
addon_ids AS (
    SELECT addon_id, addon_code FROM core.addons
)
INSERT INTO core.plan_addons (plan_id, addon_id, available)
SELECT 
    p.plan_id,
    a.addon_id,
    CASE 
        WHEN a.addon_code IN ('AD_FEE_BUSINESS', 'USER_ADDON') 
            THEN p.plan_name IN ('FREE', 'SILVER', 'GOLD')
        ELSE FALSE
    END AS available
FROM plan_ids p
CROSS JOIN addon_ids a
ON CONFLICT (plan_id, addon_id) DO UPDATE SET
    available = EXCLUDED.available;

-- Verification query
SELECT 
    sp.plan_name,
    sp.subscription_fee,
    sp.price,
    sp.max_users,
    sp.max_businesses,
    COUNT(DISTINCT CASE WHEN pf.included = TRUE THEN pf.feature_id END) AS included_features_count,
    COUNT(DISTINCT CASE WHEN pa.available = TRUE THEN pa.addon_id END) AS available_addons_count
FROM core.subscription_plans sp
LEFT JOIN core.plan_features pf ON sp.plan_id = pf.plan_id
LEFT JOIN core.plan_addons pa ON sp.plan_id = pa.plan_id
GROUP BY sp.plan_id, sp.plan_name, sp.subscription_fee, sp.price, sp.max_users, sp.max_businesses
ORDER BY sp.subscription_fee;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_plan_features_plan_id ON core.plan_features(plan_id);
CREATE INDEX IF NOT EXISTS idx_plan_features_feature_id ON core.plan_features(feature_id);
CREATE INDEX IF NOT EXISTS idx_plan_addons_plan_id ON core.plan_addons(plan_id);
CREATE INDEX IF NOT EXISTS idx_plan_addons_addon_id ON core.plan_addons(addon_id);
CREATE INDEX IF NOT EXISTS idx_features_category ON core.features(category);
CREATE INDEX IF NOT EXISTS idx_features_code ON core.features(feature_code);

COMMIT;

-- IMPORTANT NOTES:
-- 1. This script adds new columns to the existing core.subscription_plans table
-- 2. It creates new tables for features, addons, plan_features, and plan_addons
-- 3. The existing API will continue to work as it uses price, max_users, max_businesses columns
-- 4. The new columns (subscription_fee, business_ad_free_limit, etc.) are available for future use
-- 5. All data is inserted into the existing subscription_plans table, so it will be visible in the UI immediately
-- 6. The features and addons data is stored in separate tables and can be accessed via new API endpoints

