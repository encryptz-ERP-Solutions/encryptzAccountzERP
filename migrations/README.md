# Database Migrations - Quick Start Guide

## 📋 Overview

This directory contains idempotent SQL migration scripts for the ERP system. All scripts can be safely run multiple times without causing errors or data duplication.

## 📁 Files

### 1. Schema Migration
**File**: `sql/2025_11_20_create_schema.sql`
- Creates 21 database tables
- Adds 80+ indexes for performance
- Sets up foreign key constraints
- Includes comprehensive documentation

### 2. Roles & Permissions Seed
**File**: `sql/2025_11_20_seed_roles_admin.sql`
- Seeds 5 system roles:
  - **Admin**: Full system access
  - **Accountant**: Full accounting & reports
  - **Manager**: Business management & reports
  - **User**: Basic voucher & inventory access
  - **Viewer**: Read-only access
- Seeds 38 permissions across 7 modules
- Maps permissions to roles automatically

### 3. Sample Data Seed (Development Only)
**File**: `sql/2025_11_20_sample_data.sql`
- Default admin user: `admin@encryptz.com` / `Admin@123` ⚠️
- Sample business: Encryptz Technologies Pvt Ltd
- 10 Chart of Accounts entries (Assets, Liabilities, Equity, Revenue, Expense)
- 3 sample vouchers:
  - Opening Journal Entry
  - Sales Invoice
  - Payment Voucher
- Ledger entries for trial balance testing

## 🚀 Quick Start

### Prerequisites
```bash
# Ensure PostgreSQL is running
sudo systemctl status postgresql

# Connect to PostgreSQL
psql -h localhost -U postgres
```

### Run All Migrations (3 commands)

```bash
# 1. Create schema
psql -h localhost -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_create_schema.sql

# 2. Seed roles and permissions
psql -h localhost -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_seed_roles_admin.sql

# 3. Seed sample data (dev/testing only)
psql -h localhost -U postgres -d encryptz_erp \
  -f migrations/sql/2025_11_20_sample_data.sql
```

### Verify Installation

```sql
-- Check tables created
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';
-- Expected: 21

-- Check roles created
SELECT role_name FROM roles WHERE is_system_role = true ORDER BY role_name;
-- Expected: Admin, Accountant, Manager, User, Viewer

-- Check sample data
SELECT email, first_name, last_name FROM users WHERE email = 'admin@encryptz.com';
-- Expected: admin@encryptz.com, System, Administrator

-- Verify trial balance (should be balanced)
SELECT 
    SUM(debit_amount) - SUM(credit_amount) as balance 
FROM ledger_entries;
-- Expected: 0.00
```

## 🔐 Security

### Default Admin Credentials (Sample Data)
- **Email**: `admin@encryptz.com`
- **Password**: `Admin@123`
- **Hash Method**: SHA256 with `sha256_` prefix

⚠️ **CRITICAL**: 
- Do NOT run sample data seed in production
- Change default passwords immediately
- Use strong passwords in production
- Consider implementing BCrypt or PBKDF2 for production password hashing

### Generate New Password Hash

```bash
# Using Python (SHA256)
python3 -c "import hashlib; pwd='YourSecurePassword123!'; print('sha256_' + hashlib.sha256(pwd.encode()).hexdigest())"

# Update password in SQL
UPDATE users 
SET password_hash = 'sha256_your_generated_hash' 
WHERE email = 'admin@encryptz.com';
```

## 📊 Sample Data Summary

### Chart of Accounts (10 entries)
| Code | Account Name | Type | Opening Balance |
|------|--------------|------|-----------------|
| A-1000 | Cash in Hand | Asset | ₹50,000 |
| A-1010 | Bank - Current Account | Asset | ₹500,000 |
| A-1500 | Accounts Receivable | Asset | ₹150,000 |
| A-2000 | Office Equipment | Asset | ₹250,000 |
| L-1000 | Accounts Payable | Liability | ₹80,000 |
| L-1500 | GST Payable | Liability | ₹20,000 |
| E-1000 | Capital Account | Equity | ₹800,000 |
| R-1000 | Sales Revenue | Revenue | ₹0 |
| X-1000 | Salary Expense | Expense | ₹0 |
| X-2000 | Office Rent | Expense | ₹0 |

**Total Assets**: ₹950,000  
**Total Liabilities + Equity**: ₹900,000 + ₹50,000 (from opening journal) = ₹950,000  
✅ **Balanced**

### Sample Vouchers (3 entries)

1. **JV-2025-0001** (Journal Entry)
   - Date: 2025-04-01
   - Amount: ₹950,000
   - Status: Posted
   - Purpose: Opening balances

2. **SINV-2025-0001** (Sales Invoice)
   - Date: 2025-04-15
   - Amount: ₹118,000 (₹100,000 + 18% GST)
   - Status: Posted
   - Customer: ABC Corporation

3. **PAY-2025-0001** (Payment)
   - Date: 2025-04-05
   - Amount: ₹50,000
   - Status: Posted
   - Purpose: Office rent payment

## 🧪 Testing

After running migrations, you can test:

### 1. User Authentication
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@encryptz.com","password":"Admin@123"}'
```

### 2. Trial Balance Report
```sql
SELECT 
    coa.account_code,
    coa.account_name,
    coa.account_type,
    SUM(le.debit_amount) as total_debit,
    SUM(le.credit_amount) as total_credit,
    coa.opening_balance + SUM(le.debit_amount) - SUM(le.credit_amount) as closing_balance
FROM chart_of_accounts coa
LEFT JOIN ledger_entries le ON coa.account_id = le.account_id
WHERE coa.business_id = '44444444-4444-4444-4444-000000000001'
GROUP BY coa.account_id, coa.account_code, coa.account_name, coa.account_type, coa.opening_balance
ORDER BY coa.account_code;
```

### 3. Profit & Loss (Simple)
```sql
SELECT 
    account_type,
    SUM(CASE WHEN account_type = 'Revenue' THEN credit_amount - debit_amount 
             WHEN account_type = 'Expense' THEN debit_amount - credit_amount 
             ELSE 0 END) as amount
FROM ledger_entries le
JOIN chart_of_accounts coa ON le.account_id = coa.account_id
WHERE coa.business_id = '44444444-4444-4444-4444-000000000001'
  AND coa.account_type IN ('Revenue', 'Expense')
GROUP BY account_type;
```

## 📖 Documentation

For detailed migration instructions, see:
- **[Migration Runbook](../docs/migration-runbook.md)** - Complete step-by-step guide
- **[Database Schema](../docs/db-ddl.md)** - Schema documentation
- **[Schema SQL](sql/2025_11_20_create_schema.sql)** - Schema comments and notes

## 🔄 Rollback

To rollback all changes (⚠️ **deletes all data**):

```sql
-- Run the DROP statements from the schema file
-- See "DOWN MIGRATION" section in 2025_11_20_create_schema.sql
```

## 📝 Notes

1. **Idempotency**: All scripts use `ON CONFLICT` clauses and can be run multiple times
2. **UUIDs**: Hardcoded UUIDs for system data ensure consistency across environments
3. **Financial Balance**: Sample data is accounting-balanced (debits = credits)
4. **Indexes**: 80+ indexes added for query performance optimization
5. **Audit Trail**: All business tables include audit columns (created_at, created_by, etc.)
6. **Multi-tenant**: Business isolation via `business_id` in all transactional tables

## 🚨 Production Checklist

Before deploying to production:

- [ ] Backup existing database
- [ ] Test in staging environment
- [ ] Review all SQL scripts
- [ ] Do NOT run sample data seed
- [ ] Create production admin user manually
- [ ] Use strong, unique passwords
- [ ] Update connection strings
- [ ] Enable SSL for database connections
- [ ] Set up database monitoring
- [ ] Configure automated backups
- [ ] Enable audit logging
- [ ] Review and set up database user permissions

## 🆘 Support

If you encounter issues:

1. Check PostgreSQL logs: `/var/log/postgresql/`
2. Verify database connection in `appsettings.json`
3. Review the [Migration Runbook](../docs/migration-runbook.md)
4. Check for permission issues: `GRANT ALL ON DATABASE encryptz_erp TO postgres;`
5. Ensure UUID extension is enabled: `CREATE EXTENSION IF NOT EXISTS "uuid-ossp";`

---

**Created**: 2025-11-20  
**Version**: 1.0  
**Status**: Ready for development and testing
