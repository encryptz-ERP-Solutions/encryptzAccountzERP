-- =====================================================
-- ERP System - Complete Database Schema Migration
-- Date: 2025-11-20
-- Description: Creates all tables for ERP modules with UUID PKs, audit columns, and financial types
-- =====================================================

-- Prerequisites: Enable UUID extension
-- Run this first if not already enabled:
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- SECTION 1: IDENTITY & AUTHENTICATION
-- =====================================================

-- Users table - Core identity management
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    email_verified BOOLEAN DEFAULT FALSE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    phone_verified BOOLEAN DEFAULT FALSE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    is_system_admin BOOLEAN DEFAULT FALSE,
    last_login_at TIMESTAMPTZ,
    password_changed_at TIMESTAMPTZ,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until TIMESTAMPTZ,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID
);

-- Refresh tokens for JWT authentication
CREATE TABLE refresh_tokens (
    token_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    token_hash VARCHAR(255) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    revoked_by UUID,
    revoke_reason VARCHAR(255),
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- =====================================================
-- SECTION 2: ROLES & PERMISSIONS
-- =====================================================

-- Roles table - System-wide and business-specific roles
CREATE TABLE roles (
    role_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_name VARCHAR(100) NOT NULL,
    role_description TEXT,
    is_system_role BOOLEAN DEFAULT FALSE,
    business_id UUID,
    is_active BOOLEAN DEFAULT TRUE,
    permissions JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT uq_role_name_business UNIQUE(role_name, business_id)
);

-- User role assignments
CREATE TABLE user_roles (
    user_role_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    assigned_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    assigned_by UUID,
    expires_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT TRUE,
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE,
    CONSTRAINT uq_user_role UNIQUE(user_id, role_id)
);

-- Permissions catalog
CREATE TABLE permissions (
    permission_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    permission_code VARCHAR(100) NOT NULL UNIQUE,
    permission_name VARCHAR(200) NOT NULL,
    permission_description TEXT,
    module VARCHAR(100) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL
);

-- Role-Permission mapping
CREATE TABLE role_permissions (
    role_permission_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_id UUID NOT NULL,
    permission_id UUID NOT NULL,
    granted_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    granted_by UUID,
    CONSTRAINT fk_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(permission_id) ON DELETE CASCADE,
    CONSTRAINT uq_role_permission UNIQUE(role_id, permission_id)
);

-- =====================================================
-- SECTION 3: BUSINESSES & SUBSCRIPTIONS
-- =====================================================

-- Businesses table - Multi-tenant organization management
CREATE TABLE businesses (
    business_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_name VARCHAR(255) NOT NULL,
    business_type VARCHAR(50),
    legal_name VARCHAR(255),
    tax_id VARCHAR(50),
    gstin VARCHAR(15),
    pan VARCHAR(10),
    tan VARCHAR(10),
    registration_number VARCHAR(100),
    industry VARCHAR(100),
    email VARCHAR(255),
    phone VARCHAR(50),
    website VARCHAR(255),
    logo_url VARCHAR(500),
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100) DEFAULT 'India',
    postal_code VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    is_verified BOOLEAN DEFAULT FALSE,
    verified_at TIMESTAMPTZ,
    settings JSONB DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_businesses_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_businesses_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id)
);

-- Business users - Many-to-many relationship between users and businesses
CREATE TABLE business_users (
    business_user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    user_id UUID NOT NULL,
    is_owner BOOLEAN DEFAULT FALSE,
    is_primary_contact BOOLEAN DEFAULT FALSE,
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'suspended', 'invited')),
    invited_at TIMESTAMPTZ,
    joined_at TIMESTAMPTZ,
    left_at TIMESTAMPTZ,
    access_level VARCHAR(50),
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_business_users_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_business_users_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    CONSTRAINT uq_business_user UNIQUE(business_id, user_id)
);

-- Subscription plans
CREATE TABLE subscription_plans (
    plan_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    plan_name VARCHAR(100) NOT NULL UNIQUE,
    plan_code VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    billing_cycle VARCHAR(20) DEFAULT 'monthly' CHECK (billing_cycle IN ('monthly', 'quarterly', 'yearly', 'lifetime')),
    price NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    currency VARCHAR(3) DEFAULT 'INR',
    max_users INTEGER,
    max_businesses INTEGER,
    max_transactions INTEGER,
    features JSONB DEFAULT '[]',
    limits JSONB DEFAULT '{}',
    trial_days INTEGER DEFAULT 0,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID
);

-- Subscriptions - Business subscription management
CREATE TABLE subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    plan_id UUID NOT NULL,
    status VARCHAR(20) DEFAULT 'trial' CHECK (status IN ('trial', 'active', 'past_due', 'cancelled', 'expired')),
    starts_at TIMESTAMPTZ NOT NULL,
    ends_at TIMESTAMPTZ,
    trial_ends_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    cancellation_reason TEXT,
    auto_renew BOOLEAN DEFAULT TRUE,
    billing_cycle VARCHAR(20),
    amount NUMERIC(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'INR',
    payment_method VARCHAR(50),
    last_payment_at TIMESTAMPTZ,
    next_billing_at TIMESTAMPTZ,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_subscriptions_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_subscriptions_plan FOREIGN KEY (plan_id) REFERENCES subscription_plans(plan_id)
);

-- =====================================================
-- SECTION 4: CHART OF ACCOUNTS & TAX
-- =====================================================

-- Chart of accounts - Financial account structure
CREATE TABLE chart_of_accounts (
    account_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    account_code VARCHAR(50) NOT NULL,
    account_name VARCHAR(255) NOT NULL,
    account_type VARCHAR(50) NOT NULL CHECK (account_type IN ('Asset', 'Liability', 'Equity', 'Revenue', 'Expense')),
    account_subtype VARCHAR(100),
    parent_account_id UUID,
    level INTEGER DEFAULT 1,
    is_group BOOLEAN DEFAULT FALSE,
    is_system_account BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    opening_balance NUMERIC(18,2) DEFAULT 0.00,
    opening_balance_date DATE,
    current_balance NUMERIC(18,2) DEFAULT 0.00,
    currency VARCHAR(3) DEFAULT 'INR',
    description TEXT,
    tax_applicable BOOLEAN DEFAULT FALSE,
    sort_order INTEGER DEFAULT 0,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_coa_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_coa_parent FOREIGN KEY (parent_account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_coa_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_coa_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_account_code_business UNIQUE(business_id, account_code)
);

-- Tax codes - GST, TDS, and other tax configurations
CREATE TABLE tax_codes (
    tax_code_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    tax_code VARCHAR(50) NOT NULL,
    tax_name VARCHAR(200) NOT NULL,
    tax_type VARCHAR(50) NOT NULL CHECK (tax_type IN ('GST', 'IGST', 'CGST', 'SGST', 'UTGST', 'TDS', 'TCS', 'VAT', 'Other')),
    rate NUMERIC(18,4) NOT NULL,
    is_compound BOOLEAN DEFAULT FALSE,
    effective_from DATE NOT NULL,
    effective_to DATE,
    is_active BOOLEAN DEFAULT TRUE,
    tax_account_id UUID,
    description TEXT,
    hsn_sac_codes TEXT[],
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_tax_codes_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_tax_codes_account FOREIGN KEY (tax_account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_tax_codes_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_tax_codes_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_tax_code_business UNIQUE(business_id, tax_code)
);

-- =====================================================
-- SECTION 5: INVENTORY & WAREHOUSING
-- =====================================================

-- Warehouses - Storage locations
CREATE TABLE warehouses (
    warehouse_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    warehouse_code VARCHAR(50) NOT NULL,
    warehouse_name VARCHAR(200) NOT NULL,
    warehouse_type VARCHAR(50) DEFAULT 'main',
    is_active BOOLEAN DEFAULT TRUE,
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100) DEFAULT 'India',
    postal_code VARCHAR(20),
    contact_person VARCHAR(200),
    contact_phone VARCHAR(50),
    contact_email VARCHAR(255),
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_warehouses_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_warehouses_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_warehouses_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_warehouse_code_business UNIQUE(business_id, warehouse_code)
);

-- Items - Products and services catalog
CREATE TABLE items (
    item_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    item_code VARCHAR(100) NOT NULL,
    item_name VARCHAR(255) NOT NULL,
    item_type VARCHAR(50) DEFAULT 'goods' CHECK (item_type IN ('goods', 'service', 'asset', 'component')),
    category VARCHAR(100),
    sub_category VARCHAR(100),
    unit_of_measure VARCHAR(20) DEFAULT 'pcs',
    hsn_code VARCHAR(20),
    sac_code VARCHAR(20),
    barcode VARCHAR(100),
    sku VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    is_taxable BOOLEAN DEFAULT TRUE,
    is_stock_tracked BOOLEAN DEFAULT TRUE,
    description TEXT,
    purchase_price NUMERIC(18,2),
    selling_price NUMERIC(18,2),
    mrp NUMERIC(18,2),
    cost_price NUMERIC(18,2),
    minimum_stock NUMERIC(18,2) DEFAULT 0.00,
    maximum_stock NUMERIC(18,2),
    reorder_level NUMERIC(18,2),
    opening_stock NUMERIC(18,2) DEFAULT 0.00,
    current_stock NUMERIC(18,2) DEFAULT 0.00,
    default_warehouse_id UUID,
    purchase_account_id UUID,
    sales_account_id UUID,
    default_tax_code_id UUID,
    dimensions JSONB DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_items_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_items_warehouse FOREIGN KEY (default_warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT fk_items_purchase_account FOREIGN KEY (purchase_account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_items_sales_account FOREIGN KEY (sales_account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_items_tax_code FOREIGN KEY (default_tax_code_id) REFERENCES tax_codes(tax_code_id),
    CONSTRAINT fk_items_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_items_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_item_code_business UNIQUE(business_id, item_code)
);

-- Inventory movements - Stock tracking
CREATE TABLE inventory_movements (
    movement_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    item_id UUID NOT NULL,
    warehouse_id UUID NOT NULL,
    movement_type VARCHAR(50) NOT NULL CHECK (movement_type IN ('receipt', 'issue', 'transfer', 'adjustment', 'return', 'opening')),
    reference_type VARCHAR(50),
    reference_id UUID,
    movement_date DATE NOT NULL,
    quantity NUMERIC(18,2) NOT NULL,
    unit_cost NUMERIC(18,2),
    total_value NUMERIC(18,2),
    stock_before NUMERIC(18,2),
    stock_after NUMERIC(18,2),
    batch_number VARCHAR(100),
    serial_number VARCHAR(100),
    expiry_date DATE,
    notes TEXT,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_inventory_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_inventory_item FOREIGN KEY (item_id) REFERENCES items(item_id) ON DELETE CASCADE,
    CONSTRAINT fk_inventory_warehouse FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT fk_inventory_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_inventory_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id)
);

-- =====================================================
-- SECTION 6: ACCOUNTING TRANSACTIONS
-- =====================================================

-- Vouchers - Transaction headers (invoices, payments, journals, etc.)
CREATE TABLE vouchers (
    voucher_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    voucher_number VARCHAR(100) NOT NULL,
    voucher_type VARCHAR(50) NOT NULL CHECK (voucher_type IN ('Sales', 'Purchase', 'Payment', 'Receipt', 'Journal', 'Contra', 'Debit Note', 'Credit Note')),
    voucher_date DATE NOT NULL,
    reference_number VARCHAR(100),
    reference_date DATE,
    party_account_id UUID,
    party_name VARCHAR(255),
    party_gstin VARCHAR(15),
    place_of_supply VARCHAR(100),
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    tax_amount NUMERIC(18,2) DEFAULT 0.00,
    discount_amount NUMERIC(18,2) DEFAULT 0.00,
    round_off_amount NUMERIC(18,2) DEFAULT 0.00,
    net_amount NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    currency VARCHAR(3) DEFAULT 'INR',
    exchange_rate NUMERIC(18,6) DEFAULT 1.000000,
    status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'pending', 'approved', 'posted', 'cancelled', 'void')),
    posted_at TIMESTAMPTZ,
    posted_by UUID,
    narration TEXT,
    is_reverse_charge BOOLEAN DEFAULT FALSE,
    is_bill_of_supply BOOLEAN DEFAULT FALSE,
    due_date DATE,
    payment_terms VARCHAR(100),
    warehouse_id UUID,
    cost_center VARCHAR(100),
    project VARCHAR(100),
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_vouchers_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_vouchers_party FOREIGN KEY (party_account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_vouchers_warehouse FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT fk_vouchers_posted_by FOREIGN KEY (posted_by) REFERENCES users(user_id),
    CONSTRAINT fk_vouchers_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_vouchers_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_voucher_number_business UNIQUE(business_id, voucher_number, voucher_type)
);

-- Voucher lines - Line items for vouchers
CREATE TABLE voucher_lines (
    line_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    voucher_id UUID NOT NULL,
    line_number INTEGER NOT NULL,
    account_id UUID NOT NULL,
    item_id UUID,
    description TEXT,
    quantity NUMERIC(18,2),
    unit_price NUMERIC(18,2),
    discount_percentage NUMERIC(18,4),
    discount_amount NUMERIC(18,2) DEFAULT 0.00,
    taxable_amount NUMERIC(18,2) DEFAULT 0.00,
    tax_code_id UUID,
    tax_rate NUMERIC(18,4),
    tax_amount NUMERIC(18,2) DEFAULT 0.00,
    cgst_amount NUMERIC(18,2) DEFAULT 0.00,
    sgst_amount NUMERIC(18,2) DEFAULT 0.00,
    igst_amount NUMERIC(18,2) DEFAULT 0.00,
    cess_amount NUMERIC(18,2) DEFAULT 0.00,
    line_amount NUMERIC(18,2) NOT NULL,
    debit_amount NUMERIC(18,2) DEFAULT 0.00,
    credit_amount NUMERIC(18,2) DEFAULT 0.00,
    warehouse_id UUID,
    cost_center VARCHAR(100),
    project VARCHAR(100),
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    CONSTRAINT fk_voucher_lines_voucher FOREIGN KEY (voucher_id) REFERENCES vouchers(voucher_id) ON DELETE CASCADE,
    CONSTRAINT fk_voucher_lines_account FOREIGN KEY (account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_voucher_lines_item FOREIGN KEY (item_id) REFERENCES items(item_id),
    CONSTRAINT fk_voucher_lines_tax FOREIGN KEY (tax_code_id) REFERENCES tax_codes(tax_code_id),
    CONSTRAINT fk_voucher_lines_warehouse FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT uq_voucher_line_number UNIQUE(voucher_id, line_number)
);

-- Ledger entries - Posted accounting entries
CREATE TABLE ledger_entries (
    entry_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    voucher_id UUID NOT NULL,
    line_id UUID,
    entry_date DATE NOT NULL,
    account_id UUID NOT NULL,
    debit_amount NUMERIC(18,2) DEFAULT 0.00,
    credit_amount NUMERIC(18,2) DEFAULT 0.00,
    currency VARCHAR(3) DEFAULT 'INR',
    exchange_rate NUMERIC(18,6) DEFAULT 1.000000,
    base_debit_amount NUMERIC(18,2) DEFAULT 0.00,
    base_credit_amount NUMERIC(18,2) DEFAULT 0.00,
    cost_center VARCHAR(100),
    project VARCHAR(100),
    reconciliation_status VARCHAR(20) DEFAULT 'unreconciled',
    reconciled_at TIMESTAMPTZ,
    reconciled_by UUID,
    is_opening_balance BOOLEAN DEFAULT FALSE,
    narration TEXT,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    CONSTRAINT fk_ledger_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_ledger_voucher FOREIGN KEY (voucher_id) REFERENCES vouchers(voucher_id) ON DELETE CASCADE,
    CONSTRAINT fk_ledger_line FOREIGN KEY (line_id) REFERENCES voucher_lines(line_id),
    CONSTRAINT fk_ledger_account FOREIGN KEY (account_id) REFERENCES chart_of_accounts(account_id),
    CONSTRAINT fk_ledger_reconciled_by FOREIGN KEY (reconciled_by) REFERENCES users(user_id)
);

-- =====================================================
-- SECTION 7: REGISTERS & COMPLIANCE
-- =====================================================

-- Registers - Statutory registers (GSTR, Purchase, Sales, etc.)
CREATE TABLE registers (
    register_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    register_type VARCHAR(50) NOT NULL CHECK (register_type IN ('GSTR1', 'GSTR3B', 'Purchase', 'Sales', 'TDS', 'TCS', 'Inventory')),
    period_from DATE NOT NULL,
    period_to DATE NOT NULL,
    financial_year VARCHAR(10),
    status VARCHAR(20) DEFAULT 'open' CHECK (status IN ('open', 'locked', 'filed', 'revised')),
    total_transactions INTEGER DEFAULT 0,
    total_taxable_value NUMERIC(18,2) DEFAULT 0.00,
    total_tax_value NUMERIC(18,2) DEFAULT 0.00,
    total_value NUMERIC(18,2) DEFAULT 0.00,
    filed_date DATE,
    acknowledgement_number VARCHAR(100),
    filed_by UUID,
    summary_data JSONB DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_registers_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_registers_filed_by FOREIGN KEY (filed_by) REFERENCES users(user_id),
    CONSTRAINT fk_registers_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_registers_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_register_period UNIQUE(business_id, register_type, period_from, period_to)
);

-- GST submissions - GST return filing details
CREATE TABLE gst_submissions (
    submission_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    register_id UUID,
    return_type VARCHAR(20) NOT NULL CHECK (return_type IN ('GSTR1', 'GSTR3B', 'GSTR9', 'GSTR9C', 'CMP08')),
    return_period VARCHAR(10) NOT NULL,
    financial_year VARCHAR(10) NOT NULL,
    filing_date DATE,
    filing_status VARCHAR(30) DEFAULT 'not_filed' CHECK (filing_status IN ('not_filed', 'filed', 'revised', 'cancelled')),
    arn_number VARCHAR(100),
    total_sales NUMERIC(18,2) DEFAULT 0.00,
    total_purchases NUMERIC(18,2) DEFAULT 0.00,
    output_tax NUMERIC(18,2) DEFAULT 0.00,
    input_tax NUMERIC(18,2) DEFAULT 0.00,
    igst_payable NUMERIC(18,2) DEFAULT 0.00,
    cgst_payable NUMERIC(18,2) DEFAULT 0.00,
    sgst_payable NUMERIC(18,2) DEFAULT 0.00,
    cess_payable NUMERIC(18,2) DEFAULT 0.00,
    interest_amount NUMERIC(18,2) DEFAULT 0.00,
    late_fee NUMERIC(18,2) DEFAULT 0.00,
    net_payable NUMERIC(18,2) DEFAULT 0.00,
    payment_status VARCHAR(20) DEFAULT 'pending',
    payment_date DATE,
    payment_reference VARCHAR(100),
    json_data JSONB DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_gst_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_gst_register FOREIGN KEY (register_id) REFERENCES registers(register_id),
    CONSTRAINT fk_gst_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_gst_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_gst_submission UNIQUE(business_id, return_type, return_period, financial_year)
);

-- TDS submissions - TDS return filing details
CREATE TABLE tds_submissions (
    submission_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID NOT NULL,
    register_id UUID,
    quarter VARCHAR(10) NOT NULL,
    financial_year VARCHAR(10) NOT NULL,
    form_type VARCHAR(20) NOT NULL CHECK (form_type IN ('24Q', '26Q', '27Q', '27EQ')),
    filing_date DATE,
    filing_status VARCHAR(30) DEFAULT 'not_filed' CHECK (filing_status IN ('not_filed', 'filed', 'revised', 'cancelled')),
    token_number VARCHAR(100),
    total_deductees INTEGER DEFAULT 0,
    total_amount_paid NUMERIC(18,2) DEFAULT 0.00,
    total_tds_deducted NUMERIC(18,2) DEFAULT 0.00,
    tds_deposited NUMERIC(18,2) DEFAULT 0.00,
    interest_amount NUMERIC(18,2) DEFAULT 0.00,
    late_fee NUMERIC(18,2) DEFAULT 0.00,
    total_payable NUMERIC(18,2) DEFAULT 0.00,
    payment_status VARCHAR(20) DEFAULT 'pending',
    challan_details JSONB DEFAULT '[]',
    json_data JSONB DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    created_by UUID,
    updated_by UUID,
    CONSTRAINT fk_tds_business FOREIGN KEY (business_id) REFERENCES businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_tds_register FOREIGN KEY (register_id) REFERENCES registers(register_id),
    CONSTRAINT fk_tds_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT fk_tds_updated_by FOREIGN KEY (updated_by) REFERENCES users(user_id),
    CONSTRAINT uq_tds_submission UNIQUE(business_id, form_type, quarter, financial_year)
);

-- =====================================================
-- SECTION 8: AUDIT & LOGGING
-- =====================================================

-- Audit logs - System-wide audit trail
CREATE TABLE audit_logs (
    log_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    business_id UUID,
    user_id UUID,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID,
    action VARCHAR(50) NOT NULL CHECK (action IN ('CREATE', 'READ', 'UPDATE', 'DELETE', 'LOGIN', 'LOGOUT', 'EXPORT', 'IMPORT', 'APPROVE', 'REJECT', 'POST', 'VOID')),
    old_values JSONB,
    new_values JSONB,
    changes JSONB,
    ip_address VARCHAR(45),
    user_agent TEXT,
    request_id UUID,
    session_id UUID,
    timestamp TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    metadata JSONB DEFAULT '{}'
);

-- =====================================================
-- SECTION 9: INDEXES FOR PERFORMANCE
-- =====================================================

-- Users indexes
-- Reason: Email lookups for authentication are extremely frequent
CREATE INDEX idx_users_email ON users(email);
-- Reason: Filter active users in most queries
CREATE INDEX idx_users_active ON users(is_active) WHERE is_active = TRUE;
-- Reason: Support login tracking and security checks
CREATE INDEX idx_users_last_login ON users(last_login_at DESC);

-- Refresh tokens indexes
-- Reason: Token validation happens on every authenticated request
CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
-- Reason: Cleanup expired tokens efficiently
CREATE INDEX idx_refresh_tokens_expires ON refresh_tokens(expires_at);
-- Reason: Check if token is revoked during validation
CREATE INDEX idx_refresh_tokens_revoked ON refresh_tokens(revoked_at) WHERE revoked_at IS NOT NULL;

-- Roles indexes
-- Reason: Filter by business context for multi-tenant role queries
CREATE INDEX idx_roles_business ON roles(business_id);
-- Reason: Most queries need only active roles
CREATE INDEX idx_roles_active ON roles(is_active) WHERE is_active = TRUE;

-- User roles indexes
-- Reason: Lookup all roles for a user during authorization
CREATE INDEX idx_user_roles_user ON user_roles(user_id) WHERE is_active = TRUE;
-- Reason: Find all users with a specific role
CREATE INDEX idx_user_roles_role ON user_roles(role_id) WHERE is_active = TRUE;

-- Permissions indexes
-- Reason: Permission code lookups for authorization checks
CREATE INDEX idx_permissions_code ON permissions(permission_code);
-- Reason: Group permissions by module for management
CREATE INDEX idx_permissions_module ON permissions(module);

-- Role permissions indexes
-- Reason: Fetch all permissions for a role during authorization
CREATE INDEX idx_role_permissions_role ON role_permissions(role_id);

-- Businesses indexes
-- Reason: Filter active businesses for tenant isolation
CREATE INDEX idx_businesses_active ON businesses(is_active) WHERE is_active = TRUE;
-- Reason: Tax compliance queries by GSTIN
CREATE INDEX idx_businesses_gstin ON businesses(gstin) WHERE gstin IS NOT NULL;
-- Reason: Lookup businesses by owner
CREATE INDEX idx_businesses_created_by ON businesses(created_by);

-- Business users indexes
-- Reason: Find all businesses for a user (most common query pattern)
CREATE INDEX idx_business_users_user ON business_users(user_id, status);
-- Reason: Find all users in a business
CREATE INDEX idx_business_users_business ON business_users(business_id, status);
-- Reason: Quickly identify business owners
CREATE INDEX idx_business_users_owner ON business_users(business_id, is_owner) WHERE is_owner = TRUE;

-- Subscriptions indexes
-- Reason: Tenant isolation - every query filters by business_id
CREATE INDEX idx_subscriptions_business ON subscriptions(business_id);
-- Reason: Monitor subscription status for access control
CREATE INDEX idx_subscriptions_status ON subscriptions(status);
-- Reason: Billing jobs need to find subscriptions due for renewal
CREATE INDEX idx_subscriptions_next_billing ON subscriptions(next_billing_at) WHERE status = 'active';

-- Chart of accounts indexes
-- Reason: Tenant isolation - all accounting queries filter by business_id
CREATE INDEX idx_coa_business ON chart_of_accounts(business_id);
-- Reason: Account code lookups in transaction processing
CREATE INDEX idx_coa_business_code ON chart_of_accounts(business_id, account_code);
-- Reason: Build account hierarchy trees
CREATE INDEX idx_coa_parent ON chart_of_accounts(parent_account_id);
-- Reason: Filter active accounts in dropdowns and validations
CREATE INDEX idx_coa_active ON chart_of_accounts(business_id, is_active) WHERE is_active = TRUE;
-- Reason: Group accounts by type for financial reports (Trial Balance, P&L, Balance Sheet)
CREATE INDEX idx_coa_type ON chart_of_accounts(business_id, account_type);

-- Tax codes indexes
-- Reason: Tenant isolation for tax lookups
CREATE INDEX idx_tax_codes_business ON tax_codes(business_id);
-- Reason: Filter by tax type (GST, TDS) for compliance reports
CREATE INDEX idx_tax_codes_type ON tax_codes(business_id, tax_type);
-- Reason: Most queries need only active tax codes
CREATE INDEX idx_tax_codes_active ON tax_codes(business_id, is_active) WHERE is_active = TRUE;

-- Warehouses indexes
-- Reason: Tenant isolation for warehouse operations
CREATE INDEX idx_warehouses_business ON warehouses(business_id);
-- Reason: Filter active warehouses in stock queries
CREATE INDEX idx_warehouses_active ON warehouses(business_id, is_active) WHERE is_active = TRUE;

-- Items indexes
-- Reason: Tenant isolation - all item queries filter by business_id
CREATE INDEX idx_items_business ON items(business_id);
-- Reason: Item code lookups in sales/purchase transactions
CREATE INDEX idx_items_business_code ON items(business_id, item_code);
-- Reason: Filter active items in dropdowns
CREATE INDEX idx_items_active ON items(business_id, is_active) WHERE is_active = TRUE;
-- Reason: Category-based filtering for reports and catalogs
CREATE INDEX idx_items_category ON items(business_id, category);
-- Reason: HSN code required for GST reports
CREATE INDEX idx_items_hsn ON items(hsn_code) WHERE hsn_code IS NOT NULL;
-- Reason: Stock tracking and reorder reports
CREATE INDEX idx_items_stock ON items(business_id, current_stock, reorder_level) WHERE is_stock_tracked = TRUE;

-- Inventory movements indexes
-- Reason: Tenant isolation for stock reports
CREATE INDEX idx_inventory_business ON inventory_movements(business_id);
-- Reason: Item-wise stock ledger queries
CREATE INDEX idx_inventory_item ON inventory_movements(item_id, movement_date DESC);
-- Reason: Warehouse-wise stock reports
CREATE INDEX idx_inventory_warehouse ON inventory_movements(warehouse_id, movement_date DESC);
-- Reason: Date range queries for stock reports
CREATE INDEX idx_inventory_date ON inventory_movements(business_id, movement_date DESC);
-- Reason: Track movements by reference (voucher, transfer, etc.)
CREATE INDEX idx_inventory_reference ON inventory_movements(reference_type, reference_id);

-- Vouchers indexes
-- Reason: Tenant isolation - all transaction queries filter by business_id
CREATE INDEX idx_vouchers_business ON vouchers(business_id);
-- Reason: Voucher number lookups (unique constraints already create index, but explicit for clarity)
CREATE INDEX idx_vouchers_number ON vouchers(business_id, voucher_type, voucher_number);
-- Reason: Date-based queries for reports (Day Book, Ledger, P&L)
CREATE INDEX idx_vouchers_date ON vouchers(business_id, voucher_date DESC);
-- Reason: Type-based filtering (Sales, Purchase, Payment, etc.)
CREATE INDEX idx_vouchers_type ON vouchers(business_id, voucher_type, voucher_date DESC);
-- Reason: Workflow and approval queries
CREATE INDEX idx_vouchers_status ON vouchers(business_id, status);
-- Reason: Party ledger and outstanding reports
CREATE INDEX idx_vouchers_party ON vouchers(party_account_id, voucher_date DESC);
-- Reason: Posted transactions for final reports
CREATE INDEX idx_vouchers_posted ON vouchers(business_id, posted_at DESC) WHERE status = 'posted';

-- Voucher lines indexes
-- Reason: Fetch all lines for a voucher (most common query)
CREATE INDEX idx_voucher_lines_voucher ON voucher_lines(voucher_id, line_number);
-- Reason: Account-wise item analysis
CREATE INDEX idx_voucher_lines_account ON voucher_lines(account_id);
-- Reason: Item-wise sales/purchase analysis
CREATE INDEX idx_voucher_lines_item ON voucher_lines(item_id) WHERE item_id IS NOT NULL;

-- Ledger entries indexes
-- Reason: Tenant isolation for all ledger queries
CREATE INDEX idx_ledger_business ON ledger_entries(business_id);
-- Reason: Account ledger queries (most frequent - used in every account statement)
CREATE INDEX idx_ledger_account ON ledger_entries(account_id, entry_date DESC);
-- Reason: Date range queries for Trial Balance, P&L, Balance Sheet
CREATE INDEX idx_ledger_date ON ledger_entries(business_id, entry_date DESC);
-- Reason: Voucher-wise entry lookup
CREATE INDEX idx_ledger_voucher ON ledger_entries(voucher_id);
-- Reason: Bank reconciliation queries
CREATE INDEX idx_ledger_reconciliation ON ledger_entries(account_id, reconciliation_status) WHERE reconciliation_status = 'unreconciled';

-- Registers indexes
-- Reason: Tenant isolation for compliance registers
CREATE INDEX idx_registers_business ON registers(business_id);
-- Reason: Filter by register type and period for GST/TDS reports
CREATE INDEX idx_registers_type_period ON registers(business_id, register_type, period_from, period_to);
-- Reason: Financial year reports
CREATE INDEX idx_registers_fy ON registers(business_id, financial_year);

-- GST submissions indexes
-- Reason: Tenant isolation for GST compliance
CREATE INDEX idx_gst_business ON gst_submissions(business_id);
-- Reason: Return period queries for filing status
CREATE INDEX idx_gst_return_period ON gst_submissions(business_id, return_type, return_period);
-- Reason: Financial year GST summary
CREATE INDEX idx_gst_fy ON gst_submissions(business_id, financial_year);
-- Reason: Track filing status for pending returns
CREATE INDEX idx_gst_filing_status ON gst_submissions(filing_status, filing_date);

-- TDS submissions indexes
-- Reason: Tenant isolation for TDS compliance
CREATE INDEX idx_tds_business ON tds_submissions(business_id);
-- Reason: Quarter-wise TDS return queries
CREATE INDEX idx_tds_quarter ON tds_submissions(business_id, form_type, quarter, financial_year);
-- Reason: Track filing status for pending returns
CREATE INDEX idx_tds_filing_status ON tds_submissions(filing_status, filing_date);

-- Audit logs indexes
-- Reason: Tenant isolation for audit reports
CREATE INDEX idx_audit_business ON audit_logs(business_id);
-- Reason: User activity tracking
CREATE INDEX idx_audit_user ON audit_logs(user_id, timestamp DESC);
-- Reason: Entity audit trail (e.g., all changes to a voucher)
CREATE INDEX idx_audit_entity ON audit_logs(entity_type, entity_id, timestamp DESC);
-- Reason: Time-based audit queries with partitioning support
CREATE INDEX idx_audit_timestamp ON audit_logs(timestamp DESC);
-- Reason: Action-based filtering (e.g., all DELETE actions)
CREATE INDEX idx_audit_action ON audit_logs(action, timestamp DESC);

-- =====================================================
-- SECTION 10: COMMENTS ON TABLES
-- =====================================================

COMMENT ON TABLE users IS 'Core user identity and authentication';
COMMENT ON TABLE refresh_tokens IS 'JWT refresh token management for secure authentication';
COMMENT ON TABLE roles IS 'Role definitions for RBAC - both system and business-specific';
COMMENT ON TABLE permissions IS 'Permission catalog for granular access control';
COMMENT ON TABLE businesses IS 'Multi-tenant organizations with tax and legal details';
COMMENT ON TABLE business_users IS 'User-business relationships with access levels';
COMMENT ON TABLE subscriptions IS 'Business subscription and billing management';
COMMENT ON TABLE chart_of_accounts IS 'Financial account structure (Assets, Liabilities, Equity, Revenue, Expense)';
COMMENT ON TABLE tax_codes IS 'Tax rate configurations (GST, TDS, TCS) with effective dates';
COMMENT ON TABLE warehouses IS 'Storage locations for inventory management';
COMMENT ON TABLE items IS 'Product and service catalog with pricing and stock tracking';
COMMENT ON TABLE inventory_movements IS 'Stock movement tracking for all inventory transactions';
COMMENT ON TABLE vouchers IS 'Transaction headers (Sales, Purchase, Payment, Receipt, Journal)';
COMMENT ON TABLE voucher_lines IS 'Line items with amounts, tax, and item details';
COMMENT ON TABLE ledger_entries IS 'Posted accounting entries for financial reports';
COMMENT ON TABLE registers IS 'Statutory registers (GSTR, Purchase, Sales) for compliance';
COMMENT ON TABLE gst_submissions IS 'GST return filing details and status tracking';
COMMENT ON TABLE tds_submissions IS 'TDS return filing details and quarterly submissions';
COMMENT ON TABLE audit_logs IS 'Complete audit trail of all system actions';

-- =====================================================
-- POST-MIGRATION NOTES
-- =====================================================

-- 1. UUID Extension: Ensure uuid-ossp extension is enabled before running this migration
--    Command: CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
--
-- 2. Sequences vs. UUID: All primary keys use UUIDs for distributed systems support
--    UUIDs avoid sequence conflicts in multi-instance deployments
--
-- 3. Audit Columns: All business tables include created_at, updated_at, created_by, updated_by
--    Application layer must populate created_by/updated_by with current user_id
--
-- 4. Timezone: All timestamps use TIMESTAMPTZ to handle multi-region deployments
--    Always store in UTC, convert to local timezone in application layer
--
-- 5. Financial Precision: NUMERIC(18,2) used for monetary values
--    Tax rates use NUMERIC(18,4) for precision (e.g., 2.5% GST)
--
-- 6. Soft Deletes: is_active flags used instead of hard deletes for audit trail
--    Consider periodic archival of inactive records for performance
--
-- 7. JSONB Usage: metadata and settings columns allow flexible schema evolution
--    Create GIN indexes on JSONB columns if querying nested fields frequently
--
-- 8. Partitioning: Consider table partitioning for high-volume tables:
--    - audit_logs (by timestamp range)
--    - ledger_entries (by entry_date range)
--    - inventory_movements (by movement_date range)
--
-- 9. Data Seeding: After migration, seed:
--    - System roles and permissions
--    - Default subscription plans
--    - Standard chart of accounts templates
--    - Common tax codes (GST rates)
--
-- 10. Performance Tuning: Monitor and add additional indexes based on query patterns
--     Use EXPLAIN ANALYZE to identify slow queries

-- =====================================================
-- DOWN MIGRATION - DROP ALL TABLES IN REVERSE ORDER
-- =====================================================

/*
-- Execute these statements to rollback the migration
-- WARNING: This will delete all data. Ensure you have backups!

-- Drop audit logs
DROP TABLE IF EXISTS audit_logs CASCADE;

-- Drop compliance tables
DROP TABLE IF EXISTS tds_submissions CASCADE;
DROP TABLE IF EXISTS gst_submissions CASCADE;
DROP TABLE IF EXISTS registers CASCADE;

-- Drop accounting transaction tables
DROP TABLE IF EXISTS ledger_entries CASCADE;
DROP TABLE IF EXISTS voucher_lines CASCADE;
DROP TABLE IF EXISTS vouchers CASCADE;

-- Drop inventory tables
DROP TABLE IF EXISTS inventory_movements CASCADE;
DROP TABLE IF EXISTS items CASCADE;
DROP TABLE IF EXISTS warehouses CASCADE;

-- Drop account and tax tables
DROP TABLE IF EXISTS tax_codes CASCADE;
DROP TABLE IF EXISTS chart_of_accounts CASCADE;

-- Drop subscription tables
DROP TABLE IF EXISTS subscriptions CASCADE;
DROP TABLE IF EXISTS subscription_plans CASCADE;

-- Drop business tables
DROP TABLE IF EXISTS business_users CASCADE;
DROP TABLE IF EXISTS businesses CASCADE;

-- Drop RBAC tables
DROP TABLE IF EXISTS role_permissions CASCADE;
DROP TABLE IF EXISTS permissions CASCADE;
DROP TABLE IF EXISTS user_roles CASCADE;
DROP TABLE IF EXISTS roles CASCADE;

-- Drop auth tables
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Optionally drop UUID extension (only if not used by other schemas)
-- DROP EXTENSION IF EXISTS "uuid-ossp";

*/
