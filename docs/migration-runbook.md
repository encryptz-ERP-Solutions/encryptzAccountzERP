# ERP System - Database Migration Runbook

## Overview

This runbook provides step-by-step instructions for applying database migrations and seeding data for the ERP system. The migrations are designed to be idempotent, meaning they can be safely run multiple times without causing errors or duplicate data.

## Prerequisites

- PostgreSQL 12 or higher installed
- PostgreSQL `uuid-ossp` extension enabled
- Database credentials with sufficient permissions (CREATE, INSERT, UPDATE, DELETE)
- `psql` command-line tool installed (comes with PostgreSQL)
- .NET 8 SDK (optional, for Entity Framework migrations)

## Migration Files

The migration scripts are located in the `migrations/sql/` directory:

1. **2025_11_20_create_schema.sql** - Creates all database tables, indexes, and constraints
2. **2025_11_20_seed_roles_admin.sql** - Seeds system roles and permissions
3. **2025_11_20_sample_data.sql** - Seeds sample data for testing (admin user, business, COA, vouchers)

## Database Connection

### 1. Environment Variables

Set the following environment variables or update your `appsettings.json`:

```bash
export POSTGRES_HOST="localhost"
export POSTGRES_PORT="5432"
export POSTGRES_DB="encryptz_erp"
export POSTGRES_USER="postgres"
export POSTGRES_PASSWORD="your_secure_password"
```

### 2. Connection String Format

```
Host=localhost;Port=5432;Database=encryptz_erp;Username=postgres;Password=your_password;
```

## Migration Methods

### Method 1: Using psql Command Line (Recommended)

This is the most straightforward method for running SQL migrations.

#### Step 1: Connect to PostgreSQL

```bash
psql -h localhost -p 5432 -U postgres -d encryptz_erp
```

Or use the connection URL:

```bash
psql postgresql://postgres:your_password@localhost:5432/encryptz_erp
```

#### Step 2: Enable UUID Extension (One-time setup)

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

#### Step 3: Run Schema Migration

From the workspace root directory:

```bash
psql -h localhost -p 5432 -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_create_schema.sql
```

**Expected Output:**
- Multiple `CREATE TABLE` statements
- Multiple `CREATE INDEX` statements
- Schema creation completed successfully

#### Step 4: Run Roles and Permissions Seed

```bash
psql -h localhost -p 5432 -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_seed_roles_admin.sql
```

**Expected Output:**
```
NOTICE:  Seed script completed successfully:
NOTICE:    - Total Permissions: 38
NOTICE:    - System Roles: 5
NOTICE:    - Role-Permission Mappings: [count varies by role]
```

#### Step 5: Run Sample Data Seed (Development/Testing Only)

```bash
psql -h localhost -p 5432 -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_sample_data.sql
```

**Expected Output:**
```
NOTICE:  Sample data seed completed successfully:
NOTICE:    - Admin Users: 1
NOTICE:    - Sample Businesses: 1
NOTICE:    - Chart of Accounts Entries: 10
NOTICE:    - Sample Vouchers: 3
NOTICE:    - Voucher Lines: 12
NOTICE:    - Ledger Entries: 12
NOTICE:  
NOTICE:  Default Admin Credentials:
NOTICE:    Email: admin@encryptz.com
NOTICE:    Password: Admin@123
```

⚠️ **WARNING**: Do not run the sample data seed in production! It contains default credentials.

---

### Method 2: Using Bash Script

Create a migration script for convenience:

**File: `scripts/migrate.sh`**

```bash
#!/bin/bash

# Database connection settings
DB_HOST="${POSTGRES_HOST:-localhost}"
DB_PORT="${POSTGRES_PORT:-5432}"
DB_NAME="${POSTGRES_DB:-encryptz_erp}"
DB_USER="${POSTGRES_USER:-postgres}"
DB_PASSWORD="${POSTGRES_PASSWORD}"

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting ERP Database Migration...${NC}"
echo "Database: $DB_NAME on $DB_HOST:$DB_PORT"
echo ""

# Set PGPASSWORD environment variable to avoid password prompt
export PGPASSWORD="$DB_PASSWORD"

# Function to run SQL file
run_migration() {
    local file=$1
    local description=$2
    
    echo -e "${YELLOW}Running: $description${NC}"
    echo "File: $file"
    
    if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file"; then
        echo -e "${GREEN}✓ Success${NC}"
        echo ""
        return 0
    else
        echo -e "${RED}✗ Failed${NC}"
        echo ""
        return 1
    fi
}

# Step 1: Create Schema
run_migration "migrations/sql/2025_11_20_create_schema.sql" "Schema Migration"
if [ $? -ne 0 ]; then
    echo -e "${RED}Schema migration failed. Aborting.${NC}"
    exit 1
fi

# Step 2: Seed Roles and Permissions
run_migration "migrations/sql/2025_11_20_seed_roles_admin.sql" "Roles and Permissions Seed"
if [ $? -ne 0 ]; then
    echo -e "${RED}Roles seed failed. Aborting.${NC}"
    exit 1
fi

# Step 3: Seed Sample Data (optional - prompt user)
echo -e "${YELLOW}Do you want to seed sample data? (y/N)${NC}"
read -r response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    run_migration "migrations/sql/2025_11_20_sample_data.sql" "Sample Data Seed"
    if [ $? -ne 0 ]; then
        echo -e "${RED}Sample data seed failed.${NC}"
        exit 1
    fi
else
    echo "Skipping sample data seed."
fi

# Unset password
unset PGPASSWORD

echo -e "${GREEN}Migration completed successfully!${NC}"
echo ""
echo "Next steps:"
echo "1. Update appsettings.json with your database connection string"
echo "2. If you seeded sample data, login with:"
echo "   Email: admin@encryptz.com"
echo "   Password: Admin@123"
echo "3. Change the default admin password immediately!"
```

**Make the script executable:**

```bash
chmod +x scripts/migrate.sh
```

**Run the script:**

```bash
./scripts/migrate.sh
```

---

### Method 3: Using .NET Entity Framework Core (Future)

If you plan to use EF Core migrations in the future:

#### Step 1: Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

#### Step 2: Add EF Core Migration

```bash
cd CoreModule/API/encryptzERP
dotnet ef migrations add InitialCreate --project ../Infrastructure
```

#### Step 3: Apply Migration

```bash
dotnet ef database update --project ../Infrastructure
```

**Note**: The current SQL scripts are raw SQL and not EF migrations. To use EF migrations, you would need to create DbContext and entity configurations.

---

### Method 4: Using Docker Compose (Development)

If you're using Docker for development:

**File: `docker-compose.yml`** (create if it doesn't exist)

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: encryptz_erp_db
    environment:
      POSTGRES_DB: encryptz_erp
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres_dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./migrations/sql:/docker-entrypoint-initdb.d

volumes:
  postgres_data:
```

**Run with Docker:**

```bash
docker-compose up -d
```

The SQL scripts in `migrations/sql/` will be automatically executed on first startup if you mount them to `/docker-entrypoint-initdb.d`.

---

## Verification

### 1. Check Tables Created

```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

**Expected**: 21 tables (users, roles, permissions, businesses, chart_of_accounts, vouchers, etc.)

### 2. Check Roles Created

```sql
SELECT role_name, role_description, is_system_role 
FROM roles 
WHERE is_system_role = true 
ORDER BY role_name;
```

**Expected**: 5 system roles (Admin, Accountant, Manager, User, Viewer)

### 3. Check Permissions Created

```sql
SELECT module, COUNT(*) as permission_count 
FROM permissions 
GROUP BY module 
ORDER BY module;
```

**Expected**: Permissions across 7 modules

### 4. Check Sample Data (if seeded)

```sql
-- Check admin user
SELECT email, first_name, last_name, is_system_admin 
FROM users 
WHERE email = 'admin@encryptz.com';

-- Check sample business
SELECT business_name, city, is_active 
FROM businesses;

-- Check COA entries
SELECT account_code, account_name, account_type, current_balance 
FROM chart_of_accounts 
ORDER BY account_code;

-- Check vouchers
SELECT voucher_number, voucher_type, voucher_date, status, net_amount 
FROM vouchers 
ORDER BY voucher_date;

-- Verify trial balance (debits should equal credits)
SELECT 
    SUM(debit_amount) as total_debits,
    SUM(credit_amount) as total_credits,
    SUM(debit_amount) - SUM(credit_amount) as difference
FROM ledger_entries;
```

**Expected**: `difference` should be `0.00`

---

## Rollback (Emergency Only)

If you need to rollback the migrations, use the commented rollback section at the end of `2025_11_20_create_schema.sql`:

```bash
psql -h localhost -p 5432 -U postgres -d encryptz_erp -c "
-- Drop all tables in reverse order
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS tds_submissions CASCADE;
DROP TABLE IF EXISTS gst_submissions CASCADE;
DROP TABLE IF EXISTS registers CASCADE;
DROP TABLE IF EXISTS ledger_entries CASCADE;
DROP TABLE IF EXISTS voucher_lines CASCADE;
DROP TABLE IF EXISTS vouchers CASCADE;
DROP TABLE IF EXISTS inventory_movements CASCADE;
DROP TABLE IF EXISTS items CASCADE;
DROP TABLE IF EXISTS warehouses CASCADE;
DROP TABLE IF EXISTS tax_codes CASCADE;
DROP TABLE IF EXISTS chart_of_accounts CASCADE;
DROP TABLE IF EXISTS subscriptions CASCADE;
DROP TABLE IF EXISTS subscription_plans CASCADE;
DROP TABLE IF EXISTS business_users CASCADE;
DROP TABLE IF EXISTS businesses CASCADE;
DROP TABLE IF EXISTS role_permissions CASCADE;
DROP TABLE IF EXISTS permissions CASCADE;
DROP TABLE IF EXISTS user_roles CASCADE;
DROP TABLE IF EXISTS roles CASCADE;
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS users CASCADE;
"
```

⚠️ **WARNING**: This will delete ALL data. Use with extreme caution!

---

## Production Deployment

### Best Practices

1. **Backup First**: Always backup your production database before running migrations
   ```bash
   pg_dump -h localhost -U postgres encryptz_erp > backup_$(date +%Y%m%d_%H%M%S).sql
   ```

2. **Test in Staging**: Run migrations in a staging environment first

3. **Verify Migration**: Check the SQL scripts for any hardcoded data or credentials

4. **Skip Sample Data**: Do NOT run `2025_11_20_sample_data.sql` in production

5. **Change Default Passwords**: If sample data was seeded in dev, ensure no default passwords exist in production

6. **Monitor Performance**: After migration, monitor query performance and add additional indexes if needed

7. **Audit Logs**: Enable and monitor the `audit_logs` table

### Production Migration Checklist

- [ ] Backup production database
- [ ] Review all migration scripts
- [ ] Test in staging environment
- [ ] Schedule maintenance window
- [ ] Notify users of downtime
- [ ] Run schema migration
- [ ] Run roles/permissions seed
- [ ] Verify all tables and indexes created
- [ ] Create production admin user manually (do not use sample data)
- [ ] Test application connectivity
- [ ] Test critical user workflows
- [ ] Monitor error logs
- [ ] Update documentation

---

## Troubleshooting

### Issue: Permission Denied

**Error**: `ERROR: permission denied for schema public`

**Solution**: Grant necessary permissions to the database user:

```sql
GRANT ALL PRIVILEGES ON DATABASE encryptz_erp TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
```

### Issue: UUID Extension Not Available

**Error**: `ERROR: type "uuid" does not exist`

**Solution**: Enable the UUID extension:

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### Issue: Connection Refused

**Error**: `psql: error: connection to server at "localhost" (::1), port 5432 failed`

**Solution**: 
1. Check if PostgreSQL is running: `sudo systemctl status postgresql`
2. Start PostgreSQL: `sudo systemctl start postgresql`
3. Check pg_hba.conf for connection settings

### Issue: Password Authentication Failed

**Error**: `FATAL: password authentication failed for user "postgres"`

**Solution**:
1. Reset PostgreSQL password
2. Update connection string with correct password
3. Check pg_hba.conf authentication method

### Issue: Duplicate Key Violations

**Error**: `ERROR: duplicate key value violates unique constraint`

**Solution**: This is expected if running migrations multiple times. The scripts use `ON CONFLICT` clauses to handle this. If the error persists, check for data conflicts.

---

## Support

For issues or questions:
- Check the PostgreSQL logs: `/var/log/postgresql/postgresql-[version]-main.log`
- Review application logs in the .NET application
- Verify database connection in `appsettings.json`
- Consult the schema documentation in `docs/db-ddl.md`

---

## Appendix: Quick Reference Commands

### Connect to Database
```bash
psql -h localhost -U postgres -d encryptz_erp
```

### List All Tables
```sql
\dt
```

### Describe Table Structure
```sql
\d table_name
```

### Check Table Row Count
```sql
SELECT COUNT(*) FROM table_name;
```

### Export Data
```bash
pg_dump -h localhost -U postgres encryptz_erp > erp_backup.sql
```

### Import Data
```bash
psql -h localhost -U postgres encryptz_erp < erp_backup.sql
```

### Drop Database (Use with caution!)
```bash
dropdb -h localhost -U postgres encryptz_erp
```

### Create Database
```bash
createdb -h localhost -U postgres encryptz_erp
```

---

## Change Log

| Date | Version | Changes |
|------|---------|---------|
| 2025-11-20 | 1.0 | Initial migration scripts created |
| | | - Schema creation with 21 tables |
| | | - 5 system roles with 38 permissions |
| | | - Sample data for development testing |

---

**Last Updated**: 2025-11-20  
**Maintained By**: ERP Development Team
