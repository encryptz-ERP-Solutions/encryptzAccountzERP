# Migration Changelog: 0002_core_schema_improvements

## Overview
This migration adds audit tracking, user-business relationships, subscription status enum, audit logging, and performance indexes to the core schema.

## Changes

### 1. Audit Columns Added
Added audit tracking columns to the following tables:
- `core.roles`
- `core.permissions`
- `core.modules`
- `core.menu_items`
- `core.subscription_plans`

**Columns added:**
- `created_by_user_id` (UUID, nullable) - References `core.users(user_id)`
- `updated_by_user_id` (UUID, nullable) - References `core.users(user_id)`
- `created_at_utc` (TIMESTAMPTZ, NOT NULL, default: UTC now)
- `updated_at_utc` (TIMESTAMPTZ, nullable)

### 2. New Table: `core.user_businesses`
Creates a mapping table between users and businesses with support for default business selection.

**Columns:**
- `user_business_id` (UUID, PK) - Primary key
- `user_id` (UUID, NOT NULL) - References `core.users(user_id)`
- `business_id` (UUID, NOT NULL) - References `core.businesses(business_id)`
- `is_default` (BOOLEAN, NOT NULL, default: FALSE) - Indicates default business
- `created_at_utc` (TIMESTAMPTZ, NOT NULL)
- `updated_at_utc` (TIMESTAMPTZ, nullable)

**Constraints:**
- Unique partial index ensures only one default business per user
- Foreign keys with CASCADE delete

### 3. Subscription Status Enum
- Created `core.subscription_status` enum type with values:
  - `active`
  - `inactive`
  - `suspended`
  - `cancelled`
  - `expired`
  - `trial`
  - `pending`
- Converted `core.user_subscriptions.status` column from VARCHAR to enum type
- Invalid existing values are converted to `pending` before migration

### 4. New Table: `core.audit_logs`
Simple audit trail table for tracking changes to core tables.

**Columns:**
- `audit_log_id` (BIGSERIAL, PK)
- `table_name` (VARCHAR(100), NOT NULL)
- `record_id` (VARCHAR(255), NOT NULL)
- `action` (VARCHAR(20), NOT NULL) - CHECK constraint: INSERT, UPDATE, DELETE
- `changed_by_user_id` (UUID, nullable) - References `core.users(user_id)`
- `changed_at_utc` (TIMESTAMPTZ, NOT NULL)
- `old_values` (JSONB, nullable)
- `new_values` (JSONB, nullable)
- `ip_address` (INET, nullable)
- `user_agent` (TEXT, nullable)

### 5. Performance Indexes
Added indexes on foreign key columns for common join operations:

**Foreign Key Indexes:**
- `user_subscriptions`: business_id, plan_id, status
- `menu_items`: module_id, parent_menu_item_id
- `permissions`: module_id, menu_item_id
- `role_permissions`: role_id, permission_id
- `subscription_plan_permissions`: plan_id, permission_id
- `user_business_roles`: user_id, business_id, role_id
- `user_businesses`: user_id, business_id

**Audit Column Indexes:**
- Partial indexes on `created_by_user_id` and `updated_by_user_id` for all tables with audit columns (only where values are NOT NULL)

## Migration Characteristics

- **Idempotent**: Safe to run multiple times using `IF NOT EXISTS` checks
- **Transactional**: Changes wrapped in BEGIN/COMMIT blocks
- **Non-blocking**: Audit columns are nullable to avoid blocking on existing data
- **Safe**: Uses `IF NOT EXISTS` and conditional checks throughout

## Rollback
A commented rollback section is included in the migration file. Uncomment and execute to revert changes (with data loss warnings).

## Dependencies
- PostgreSQL 14+
- `pgcrypto` extension (for UUID generation)
- Existing core schema tables must exist

## Testing Recommendations
1. Test on a staging database first
2. Verify enum conversion handles all existing status values
3. Check that audit columns are properly nullable
4. Verify unique constraint on `user_businesses.is_default` works correctly
5. Test rollback procedure on a copy of production data

