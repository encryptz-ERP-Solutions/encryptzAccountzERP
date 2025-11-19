/*
================================================================================================
Filename: encryptzERP_Acct_Schema_PostgreSQL.sql
Description: PostgreSQL database schema for the Accounting Module of the Encryptz ERP.
Version: 2.0
Database: PostgreSQL 14+
================================================================================================
*/

-- Create the 'acct' schema if it doesn't already exist
CREATE SCHEMA IF NOT EXISTS acct;

-- =================================================================================================
-- Section 1: Chart of Accounts - The foundation of the accounting system.
-- =================================================================================================

-- Lookup table for the 5 main account types in accounting.
CREATE TABLE IF NOT EXISTS acct.account_types (
    account_type_id SERIAL PRIMARY KEY,
    account_type_name VARCHAR(50) NOT NULL, -- e.g., Asset, Liability, Equity, Revenue, Expense
    normal_balance CHAR(2) NOT NULL -- 'Dr' for Debit, 'Cr' for Credit
);

-- Seed the fundamental account types (only if they don't exist)
INSERT INTO acct.account_types (account_type_name, normal_balance)
SELECT 'Asset', 'Dr'
WHERE NOT EXISTS (SELECT 1 FROM acct.account_types WHERE account_type_name = 'Asset');

INSERT INTO acct.account_types (account_type_name, normal_balance)
SELECT 'Liability', 'Cr'
WHERE NOT EXISTS (SELECT 1 FROM acct.account_types WHERE account_type_name = 'Liability');

INSERT INTO acct.account_types (account_type_name, normal_balance)
SELECT 'Equity', 'Cr'
WHERE NOT EXISTS (SELECT 1 FROM acct.account_types WHERE account_type_name = 'Equity');

INSERT INTO acct.account_types (account_type_name, normal_balance)
SELECT 'Revenue', 'Cr'
WHERE NOT EXISTS (SELECT 1 FROM acct.account_types WHERE account_type_name = 'Revenue');

INSERT INTO acct.account_types (account_type_name, normal_balance)
SELECT 'Expense', 'Dr'
WHERE NOT EXISTS (SELECT 1 FROM acct.account_types WHERE account_type_name = 'Expense');

-- The Chart of Accounts for a specific business.
CREATE TABLE IF NOT EXISTS acct.chart_of_accounts (
    account_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    business_id UUID NOT NULL,
    account_type_id INTEGER NOT NULL,
    parent_account_id UUID NULL, -- For creating hierarchical accounts (e.g., 'Current Assets' is parent of 'Cash')
    account_code VARCHAR(20) NOT NULL, -- e.g., '1010' for Cash
    account_name VARCHAR(200) NOT NULL, -- e.g., 'Cash in Bank'
    description VARCHAR(500) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_system_account BOOLEAN NOT NULL DEFAULT FALSE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at_utc TIMESTAMPTZ NULL,

    CONSTRAINT fk_chart_of_accounts_business_id FOREIGN KEY (business_id) REFERENCES core.businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_chart_of_accounts_account_type_id FOREIGN KEY (account_type_id) REFERENCES acct.account_types(account_type_id),
    CONSTRAINT fk_chart_of_accounts_parent_account_id FOREIGN KEY (parent_account_id) REFERENCES acct.chart_of_accounts(account_id)
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_business_id ON acct.chart_of_accounts(business_id);
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_account_type_id ON acct.chart_of_accounts(account_type_id);
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_parent_account_id ON acct.chart_of_accounts(parent_account_id);
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_account_code ON acct.chart_of_accounts(business_id, account_code);

-- =================================================================================================
-- Section 2: Transaction Management
-- =================================================================================================

-- Transaction Header: Represents a single accounting transaction (e.g., a journal entry)
CREATE TABLE IF NOT EXISTS acct.transaction_headers (
    transaction_header_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    business_id UUID NOT NULL,
    transaction_date DATE NOT NULL,
    reference_number VARCHAR(50) NULL,
    description TEXT NOT NULL,
    created_by_user_id UUID NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),

    CONSTRAINT fk_transaction_headers_business_id FOREIGN KEY (business_id) REFERENCES core.businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_transaction_headers_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id)
);

-- Transaction Detail: Individual debit/credit lines within a transaction
CREATE TABLE IF NOT EXISTS acct.transaction_details (
    transaction_detail_id BIGSERIAL PRIMARY KEY,
    transaction_header_id UUID NOT NULL,
    account_id UUID NOT NULL,
    debit_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    credit_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,

    CONSTRAINT fk_transaction_details_transaction_header_id FOREIGN KEY (transaction_header_id) REFERENCES acct.transaction_headers(transaction_header_id) ON DELETE CASCADE,
    CONSTRAINT fk_transaction_details_account_id FOREIGN KEY (account_id) REFERENCES acct.chart_of_accounts(account_id),
    CONSTRAINT chk_transaction_details_debit_credit CHECK (
        (debit_amount = 0 AND credit_amount > 0) OR 
        (debit_amount > 0 AND credit_amount = 0)
    )
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS ix_transaction_headers_business_id ON acct.transaction_headers(business_id);
CREATE INDEX IF NOT EXISTS ix_transaction_headers_transaction_date ON acct.transaction_headers(transaction_date);
CREATE INDEX IF NOT EXISTS ix_transaction_headers_created_by_user_id ON acct.transaction_headers(created_by_user_id);
CREATE INDEX IF NOT EXISTS ix_transaction_details_transaction_header_id ON acct.transaction_details(transaction_header_id);
CREATE INDEX IF NOT EXISTS ix_transaction_details_account_id ON acct.transaction_details(account_id);

