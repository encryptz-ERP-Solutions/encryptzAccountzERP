/*
================================================================================================
Migration: 0002_core_api_support.sql
Description: Ensures all schema changes from 0002_core_schema_improvements are in place
             and adds any additional indexes/constraints needed for API operations.
Version: 1.0
Database: PostgreSQL 14+
Requires: pgcrypto extension
Note: This migration is idempotent and safe to run multiple times.
================================================================================================
*/

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- =================================================================================================
-- Step 1: Ensure audit columns exist on all target tables
-- =================================================================================================

BEGIN;

-- Verify and add audit columns to core.roles (if not already added by previous migration)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'roles' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.roles 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

-- Verify and add audit columns to core.permissions
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'permissions' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.permissions 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

-- Verify and add audit columns to core.modules
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'modules' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.modules 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

-- Verify and add audit columns to core.menu_items
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'menu_items' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.menu_items 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

-- Verify and add audit columns to core.subscription_plans
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'subscription_plans' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.subscription_plans 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

-- Verify businesses table has audit columns (may already exist)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'businesses' 
        AND column_name = 'created_by_user_id'
    ) THEN
        ALTER TABLE core.businesses 
        ADD COLUMN created_by_user_id UUID NULL,
        ADD COLUMN updated_by_user_id UUID NULL,
        ADD COLUMN created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
        ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
    END IF;
END $$;

COMMIT;

-- =================================================================================================
-- Step 2: Ensure foreign key constraints exist for audit columns
-- =================================================================================================

BEGIN;

-- Add FK constraints for roles (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_roles_created_by_user_id'
    ) THEN
        ALTER TABLE core.roles 
        ADD CONSTRAINT fk_roles_created_by_user_id 
        FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_roles_updated_by_user_id'
    ) THEN
        ALTER TABLE core.roles 
        ADD CONSTRAINT fk_roles_updated_by_user_id 
        FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

-- Add FK constraints for permissions
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_permissions_created_by_user_id'
    ) THEN
        ALTER TABLE core.permissions 
        ADD CONSTRAINT fk_permissions_created_by_user_id 
        FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_permissions_updated_by_user_id'
    ) THEN
        ALTER TABLE core.permissions 
        ADD CONSTRAINT fk_permissions_updated_by_user_id 
        FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

-- Add FK constraints for modules
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_modules_created_by_user_id'
    ) THEN
        ALTER TABLE core.modules 
        ADD CONSTRAINT fk_modules_created_by_user_id 
        FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_modules_updated_by_user_id'
    ) THEN
        ALTER TABLE core.modules 
        ADD CONSTRAINT fk_modules_updated_by_user_id 
        FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

-- Add FK constraints for menu_items
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_menu_items_created_by_user_id'
    ) THEN
        ALTER TABLE core.menu_items 
        ADD CONSTRAINT fk_menu_items_created_by_user_id 
        FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_menu_items_updated_by_user_id'
    ) THEN
        ALTER TABLE core.menu_items 
        ADD CONSTRAINT fk_menu_items_updated_by_user_id 
        FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

-- Add FK constraints for subscription_plans
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_subscription_plans_created_by_user_id'
    ) THEN
        ALTER TABLE core.subscription_plans 
        ADD CONSTRAINT fk_subscription_plans_created_by_user_id 
        FOREIGN KEY (created_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_subscription_plans_updated_by_user_id'
    ) THEN
        ALTER TABLE core.subscription_plans 
        ADD CONSTRAINT fk_subscription_plans_updated_by_user_id 
        FOREIGN KEY (updated_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

COMMIT;

-- =================================================================================================
-- Step 3: Ensure user_businesses table exists
-- =================================================================================================

BEGIN;

CREATE TABLE IF NOT EXISTS core.user_businesses (
    user_business_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    business_id UUID NOT NULL,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at_utc TIMESTAMPTZ NULL,
    
    CONSTRAINT fk_user_businesses_user_id 
        FOREIGN KEY (user_id) REFERENCES core.users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_businesses_business_id 
        FOREIGN KEY (business_id) REFERENCES core.businesses(business_id) ON DELETE CASCADE
);

-- Create unique index to ensure only one default business per user
CREATE UNIQUE INDEX IF NOT EXISTS idx_user_businesses_user_default 
    ON core.user_businesses(user_id) 
    WHERE is_default = TRUE;

-- Create indexes for common lookups
CREATE INDEX IF NOT EXISTS idx_user_businesses_user_id 
    ON core.user_businesses(user_id);
CREATE INDEX IF NOT EXISTS idx_user_businesses_business_id 
    ON core.user_businesses(business_id);

COMMIT;

-- =================================================================================================
-- Step 4: Ensure subscription_status enum exists and user_subscriptions.status uses it
-- =================================================================================================

BEGIN;

-- Create enum type if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'subscription_status') THEN
        CREATE TYPE core.subscription_status AS ENUM (
            'active',
            'inactive',
            'suspended',
            'cancelled',
            'expired',
            'trial',
            'pending',
            'past_due'
        );
    END IF;
END $$;

-- Convert status column to enum type (if not already converted)
DO $$
BEGIN
    -- Check if column exists and is not already the enum type
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'user_subscriptions' 
        AND column_name = 'status'
        AND data_type != 'USER-DEFINED'
    ) THEN
        -- First, ensure all existing values are valid enum values
        -- Update any invalid values to 'inactive' as fallback
        UPDATE core.user_subscriptions 
        SET status = 'inactive' 
        WHERE status NOT IN ('active', 'inactive', 'suspended', 'cancelled', 'expired', 'trial', 'pending', 'past_due');
        
        -- Convert the column to enum type
        ALTER TABLE core.user_subscriptions 
        ALTER COLUMN status TYPE core.subscription_status 
        USING status::core.subscription_status;
    END IF;
END $$;

COMMIT;

-- =================================================================================================
-- Step 5: Ensure audit_logs table exists
-- =================================================================================================

BEGIN;

CREATE TABLE IF NOT EXISTS core.audit_logs (
    audit_log_id BIGSERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    record_id VARCHAR(255) NOT NULL,
    action VARCHAR(20) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE')),
    changed_by_user_id UUID NULL,
    changed_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    old_values JSONB NULL,
    new_values JSONB NULL,
    ip_address INET NULL,
    user_agent TEXT NULL
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS idx_audit_logs_table_record 
    ON core.audit_logs(table_name, record_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_by 
    ON core.audit_logs(changed_by_user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_at 
    ON core.audit_logs(changed_at_utc DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action 
    ON core.audit_logs(action);

-- Add foreign key if users table exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'core' 
        AND constraint_name = 'fk_audit_logs_changed_by_user_id'
    ) THEN
        ALTER TABLE core.audit_logs 
        ADD CONSTRAINT fk_audit_logs_changed_by_user_id 
        FOREIGN KEY (changed_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
    END IF;
END $$;

COMMIT;

-- =================================================================================================
-- Step 6: Ensure performance indexes exist
-- =================================================================================================

BEGIN;

-- Indexes for user_subscriptions
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_business_id 
    ON core.user_subscriptions(business_id);
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_plan_id 
    ON core.user_subscriptions(plan_id);
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_status 
    ON core.user_subscriptions(status);

-- Indexes for menu_items
CREATE INDEX IF NOT EXISTS idx_menu_items_module_id 
    ON core.menu_items(module_id);
CREATE INDEX IF NOT EXISTS idx_menu_items_parent_menu_item_id 
    ON core.menu_items(parent_menu_item_id);

-- Indexes for permissions
CREATE INDEX IF NOT EXISTS idx_permissions_module_id 
    ON core.permissions(module_id);
CREATE INDEX IF NOT EXISTS idx_permissions_menu_item_id 
    ON core.permissions(menu_item_id);

-- Indexes for role_permissions
CREATE INDEX IF NOT EXISTS idx_role_permissions_role_id 
    ON core.role_permissions(role_id);
CREATE INDEX IF NOT EXISTS idx_role_permissions_permission_id 
    ON core.role_permissions(permission_id);

-- Indexes for subscription_plan_permissions
CREATE INDEX IF NOT EXISTS idx_subscription_plan_permissions_plan_id 
    ON core.subscription_plan_permissions(plan_id);
CREATE INDEX IF NOT EXISTS idx_subscription_plan_permissions_permission_id 
    ON core.subscription_plan_permissions(permission_id);

-- Indexes for user_business_roles
CREATE INDEX IF NOT EXISTS idx_user_business_roles_user_id 
    ON core.user_business_roles(user_id);
CREATE INDEX IF NOT EXISTS idx_user_business_roles_business_id 
    ON core.user_business_roles(business_id);
CREATE INDEX IF NOT EXISTS idx_user_business_roles_role_id 
    ON core.user_business_roles(role_id);

-- Indexes for audit columns (created_by/updated_by lookups)
CREATE INDEX IF NOT EXISTS idx_roles_created_by_user_id 
    ON core.roles(created_by_user_id) WHERE created_by_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_roles_updated_by_user_id 
    ON core.roles(updated_by_user_id) WHERE updated_by_user_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_permissions_created_by_user_id 
    ON core.permissions(created_by_user_id) WHERE created_by_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_permissions_updated_by_user_id 
    ON core.permissions(updated_by_user_id) WHERE updated_by_user_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_modules_created_by_user_id 
    ON core.modules(created_by_user_id) WHERE created_by_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_modules_updated_by_user_id 
    ON core.modules(updated_by_user_id) WHERE updated_by_user_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_menu_items_created_by_user_id 
    ON core.menu_items(created_by_user_id) WHERE created_by_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_menu_items_updated_by_user_id 
    ON core.menu_items(updated_by_user_id) WHERE updated_by_user_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_subscription_plans_created_by_user_id 
    ON core.subscription_plans(created_by_user_id) WHERE created_by_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_subscription_plans_updated_by_user_id 
    ON core.subscription_plans(updated_by_user_id) WHERE updated_by_user_id IS NOT NULL;

COMMIT;

-- =================================================================================================
-- Migration Complete
-- =================================================================================================

DO $$
BEGIN
    RAISE NOTICE 'Migration 0002_core_api_support completed successfully';
END $$;

