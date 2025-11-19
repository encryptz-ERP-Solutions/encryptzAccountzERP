# Database Migration Guide

## Migration: 0002_core_schema_improvements.sql

This migration adds audit tracking, user-business relationships, subscription status enum, audit logging, and performance indexes to the core schema.

## Prerequisites

- PostgreSQL 14 or higher
- `pgcrypto` extension available
- Existing core schema tables (users, businesses, roles, permissions, modules, menu_items, subscription_plans, user_subscriptions, etc.)
- Database backup (see below)

## Pre-Migration Steps

### 1. Backup Your Database

**Always backup your database before running migrations:**

```bash
# Create a backup with timestamp
pg_dump -h localhost -U your_username -d encryptzERPCore -F c -f backup_$(date +%Y%m%d_%H%M%S).dump

# Or as SQL file
pg_dump -h localhost -U your_username -d encryptzERPCore -f backup_$(date +%Y%m%d_%H%M%S).sql
```

### 2. Test in Staging First

**Never run migrations directly on production without testing:**

```bash
# Restore to staging database
createdb -h localhost -U your_username encryptzERPCore_staging
pg_restore -h localhost -U your_username -d encryptzERPCore_staging backup_*.dump

# Run migration on staging
psql -h localhost -U your_username -d encryptzERPCore_staging -f 0002_core_schema_improvements.sql
```

## Running the Migration

### Option 1: Using psql (Recommended)

```bash
# Connect to your database and run the migration
psql -h localhost -U your_username -d encryptzERPCore -f 0002_core_schema_improvements.sql

# Or with explicit transaction (migration already uses transactions internally)
psql -h localhost -U your_username -d encryptzERPCore -1 -f 0002_core_schema_improvements.sql
```

### Option 2: Using psql with Transaction Wrapper

```bash
# Wrap entire migration in a single transaction
psql -h localhost -U your_username -d encryptzERPCore <<EOF
BEGIN;
\i 0002_core_schema_improvements.sql
COMMIT;
EOF
```

### Option 3: Using pgAdmin or DBeaver

1. Open the SQL file in your database client
2. Connect to your database
3. Execute the script (most clients will run it in a transaction by default)

## Verification

After running the migration, verify the changes:

```sql
-- Check audit columns were added
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_schema = 'core' 
AND table_name IN ('roles', 'permissions', 'modules', 'menu_items', 'subscription_plans')
AND column_name LIKE '%user_id' OR column_name LIKE '%_at_utc'
ORDER BY table_name, column_name;

-- Verify user_businesses table exists
SELECT * FROM information_schema.tables 
WHERE table_schema = 'core' AND table_name = 'user_businesses';

-- Check enum type was created
SELECT typname, typtype FROM pg_type 
WHERE typname = 'subscription_status';

-- Verify status column type
SELECT column_name, data_type, udt_name 
FROM information_schema.columns 
WHERE table_schema = 'core' 
AND table_name = 'user_subscriptions' 
AND column_name = 'status';

-- Check audit_logs table
SELECT * FROM information_schema.tables 
WHERE table_schema = 'core' AND table_name = 'audit_logs';

-- List new indexes
SELECT schemaname, tablename, indexname 
FROM pg_indexes 
WHERE schemaname = 'core' 
AND indexname LIKE 'idx_%'
ORDER BY tablename, indexname;
```

## Rollback

If you need to rollback the migration:

1. **Review the rollback section** in the migration file (commented out at the bottom)
2. **Uncomment the rollback statements** you need
3. **Test rollback on staging first**
4. **Backup again** before rolling back production

```bash
# Example rollback (after uncommenting in file)
psql -h localhost -U your_username -d encryptzERPCore -f 0002_core_schema_improvements.sql
```

**Warning:** Rollback will drop tables and columns, potentially causing data loss. Always backup first.

## Troubleshooting

### Error: "extension pgcrypto does not exist"
```sql
-- Install extension (requires superuser or database owner)
CREATE EXTENSION IF NOT EXISTS pgcrypto;
```

### Error: "relation does not exist"
- Ensure all core schema tables exist before running migration
- Check that you're connected to the correct database
- Verify schema name is 'core' (case-sensitive in PostgreSQL)

### Error: "column already exists"
- Migration is idempotent and should handle this
- If error persists, check for conflicting column names

### Error: "invalid input value for enum"
- Migration converts invalid status values to 'pending'
- Check existing data: `SELECT DISTINCT status FROM core.user_subscriptions;`
- Manually update any problematic values before migration

## Post-Migration

1. **Update application code** to populate audit columns (`created_by_user_id`, `updated_by_user_id`, `updated_at_utc`)
2. **Implement audit logging** to write to `core.audit_logs` table
3. **Update business logic** to use `core.user_businesses` table for user-business relationships
4. **Update queries** to use enum type for subscription status (application should handle enum values)

## Support

For issues or questions:
1. Check migration logs for specific error messages
2. Review the changelog: `CHANGELOG_0002.md`
3. Test on a copy of production data in staging

