/*
================================================================================================
Filename: encryptzERPCore_Schema_PostgreSQL.sql
Description: PostgreSQL database schema for the Core Module of the Encryptz ERP.
Version: 5.0
Database: PostgreSQL 14+
Requires: pgcrypto extension
================================================================================================
*/

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Create schemas if they don't exist
CREATE SCHEMA IF NOT EXISTS core;
CREATE SCHEMA IF NOT EXISTS admin;
CREATE SCHEMA IF NOT EXISTS acct;

-- =================================================================================================
-- Section 1: Core Identity (Users & Businesses)
-- =================================================================================================

CREATE TABLE IF NOT EXISTS core.users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_handle VARCHAR(50) NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    email VARCHAR(256) NULL,
    hashed_password TEXT NULL,
    mobile_country_code VARCHAR(10) NULL,
    mobile_number VARCHAR(20) NULL,
    pan_card_number_encrypted BYTEA NOT NULL,
    aadhar_number_encrypted BYTEA NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at_utc TIMESTAMPTZ NULL
);

-- Add unique constraints
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_user_handle ON core.users(user_handle);
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_email ON core.users(email) WHERE email IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_mobile ON core.users(mobile_country_code, mobile_number) WHERE mobile_number IS NOT NULL;

-- Add computed column for PAN card hash (PostgreSQL uses generated columns)
ALTER TABLE core.users DROP COLUMN IF EXISTS pan_card_number_hash;
ALTER TABLE core.users ADD COLUMN pan_card_number_hash BYTEA GENERATED ALWAYS AS (digest(pan_card_number_encrypted, 'sha256')) STORED;
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_pan_card_number_hash ON core.users(pan_card_number_hash);

CREATE TABLE IF NOT EXISTS core.businesses (
    business_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    business_name VARCHAR(250) NOT NULL,
    business_code VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    gstin VARCHAR(15) NULL,
    tan_number VARCHAR(10) NULL,
    address_line1 VARCHAR(250) NULL,
    address_line2 VARCHAR(250) NULL,
    city VARCHAR(100) NULL,
    state_id INTEGER NULL,
    pin_code VARCHAR(10) NULL,
    country_id INTEGER NULL,
    created_by_user_id UUID NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_by_user_id UUID NOT NULL,
    updated_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),

    CONSTRAINT fk_businesses_created_by FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id),
    CONSTRAINT fk_businesses_updated_by FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_businesses_business_code ON core.businesses(business_code);

-- =================================================================================================
-- Section 2: Subscription and Plan Management
-- =================================================================================================

CREATE TABLE IF NOT EXISTS core.subscription_plans (
    plan_id SERIAL PRIMARY KEY,
    plan_name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    price DECIMAL(18, 2) NOT NULL DEFAULT 0,
    max_users INTEGER NOT NULL,
    max_businesses INTEGER NOT NULL,
    is_publicly_visible BOOLEAN NOT NULL DEFAULT TRUE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS core.user_subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    business_id UUID NOT NULL,
    plan_id INTEGER NOT NULL,
    status VARCHAR(50) NOT NULL,
    start_date_utc TIMESTAMPTZ NOT NULL,
    end_date_utc TIMESTAMPTZ NOT NULL,
    trial_ends_at_utc TIMESTAMPTZ NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),

    CONSTRAINT fk_user_subscriptions_business_id FOREIGN KEY (business_id) REFERENCES core.businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_subscriptions_plan_id FOREIGN KEY (plan_id) REFERENCES core.subscription_plans(plan_id)
);

-- =================================================================================================
-- Section 3: RBAC (Role-Based Access Control)
-- =================================================================================================

CREATE TABLE IF NOT EXISTS core.modules (
    module_id SERIAL PRIMARY KEY,
    module_name VARCHAR(100) NOT NULL,
    is_system_module BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS core.menu_items (
    menu_item_id SERIAL PRIMARY KEY,
    module_id INTEGER NOT NULL,
    parent_menu_item_id INTEGER NULL,
    menu_text VARCHAR(100) NOT NULL,
    menu_url VARCHAR(250) NULL,
    icon_class VARCHAR(100) NULL,
    display_order INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT fk_menu_items_module_id FOREIGN KEY (module_id) REFERENCES core.modules(module_id),
    CONSTRAINT fk_menu_items_parent_menu_item_id FOREIGN KEY (parent_menu_item_id) REFERENCES core.menu_items(menu_item_id)
);

CREATE TABLE IF NOT EXISTS core.permissions (
    permission_id SERIAL PRIMARY KEY,
    permission_key VARCHAR(100) NOT NULL,
    description VARCHAR(500) NOT NULL,
    menu_item_id INTEGER NULL,
    module_id INTEGER NOT NULL,

    CONSTRAINT fk_permissions_menu_item_id FOREIGN KEY (menu_item_id) REFERENCES core.menu_items(menu_item_id),
    CONSTRAINT fk_permissions_module_id FOREIGN KEY (module_id) REFERENCES core.modules(module_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_permissions_permission_key ON core.permissions(permission_key);

CREATE TABLE IF NOT EXISTS core.roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    is_system_role BOOLEAN NOT NULL DEFAULT FALSE
);

-- Junction table to map Permissions to Roles
CREATE TABLE IF NOT EXISTS core.role_permissions (
    role_id INTEGER NOT NULL,
    permission_id INTEGER NOT NULL,
    PRIMARY KEY (role_id, permission_id),
    CONSTRAINT fk_role_permissions_role_id FOREIGN KEY (role_id) REFERENCES core.roles(role_id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES core.permissions(permission_id) ON DELETE CASCADE
);

-- Junction table to map Permissions to Subscription Plans
CREATE TABLE IF NOT EXISTS core.subscription_plan_permissions (
    plan_id INTEGER NOT NULL,
    permission_id INTEGER NOT NULL,
    PRIMARY KEY (plan_id, permission_id),
    CONSTRAINT fk_subscription_plan_permissions_plan_id FOREIGN KEY (plan_id) REFERENCES core.subscription_plans(plan_id) ON DELETE CASCADE,
    CONSTRAINT fk_subscription_plan_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES core.permissions(permission_id) ON DELETE CASCADE
);

-- Junction table to map a User to a Role within a specific Business
CREATE TABLE IF NOT EXISTS core.user_business_roles (
    user_id UUID NOT NULL,
    business_id UUID NOT NULL,
    role_id INTEGER NOT NULL,
    PRIMARY KEY (user_id, business_id, role_id),
    CONSTRAINT fk_user_business_roles_user_id FOREIGN KEY (user_id) REFERENCES core.users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_business_roles_business_id FOREIGN KEY (business_id) REFERENCES core.businesses(business_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_business_roles_role_id FOREIGN KEY (role_id) REFERENCES core.roles(role_id) ON DELETE CASCADE
);

-- =================================================================================================
-- Section 4: Admin Schema - One Time Passwords
-- =================================================================================================

CREATE TABLE IF NOT EXISTS admin.one_time_passwords (
    otp_id BIGSERIAL PRIMARY KEY,
    login_identifier VARCHAR(100) NOT NULL,
    otp VARCHAR(6) NOT NULL,
    expiry_time_utc TIMESTAMPTZ NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

-- Add an index for faster lookups on the login identifier
CREATE INDEX IF NOT EXISTS ix_one_time_passwords_login_identifier ON admin.one_time_passwords(login_identifier);

-- =================================================================================================
-- Section 5: Seed Initial Data
-- =================================================================================================

-- Seed default system roles (only if they don't exist)
INSERT INTO core.roles (role_name, description, is_system_role)
SELECT 'Business Owner', 'Has ultimate control over the business account, including subscription and billing.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM core.roles WHERE role_name = 'Business Owner');

INSERT INTO core.roles (role_name, description, is_system_role)
SELECT 'Admin', 'Can manage users and has full access to all modules within the business.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM core.roles WHERE role_name = 'Admin');

INSERT INTO core.roles (role_name, description, is_system_role)
SELECT 'Accountant', 'Has access to the Accounts module for managing transactions, ledgers, and reports.', FALSE
WHERE NOT EXISTS (SELECT 1 FROM core.roles WHERE role_name = 'Accountant');

INSERT INTO core.roles (role_name, description, is_system_role)
SELECT 'Sales Staff', 'Has access to CRM and Point of Sale (POS) modules.', FALSE
WHERE NOT EXISTS (SELECT 1 FROM core.roles WHERE role_name = 'Sales Staff');

INSERT INTO core.roles (role_name, description, is_system_role)
SELECT 'Read Only', 'Can view data and reports but cannot create or edit records.', FALSE
WHERE NOT EXISTS (SELECT 1 FROM core.roles WHERE role_name = 'Read Only');

