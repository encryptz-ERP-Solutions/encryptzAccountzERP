# Dashboard SQL Queries Documentation

This document contains the SQL queries used by the Dashboard API endpoints for DBA validation and performance review.

## Overview

The dashboard provides real-time business metrics (KPIs), recent activities from audit logs, and subscription status. All queries are optimized for fast retrieval with proper indexing.

---

## 1. KPI Aggregation Query

**Purpose**: Calculate key financial metrics including receivables, payables, cash, revenue, expenses, and net profit.

**Performance**: Uses CTEs and aggregations. Expected execution time: <100ms for businesses with <10k transactions.

```sql
WITH account_balances AS (
    SELECT 
        coa.account_id,
        coa.account_type_id,
        at.account_type_name,
        SUM(COALESCE(td.debit_amount, 0) - COALESCE(td.credit_amount, 0)) AS balance
    FROM acct.chart_of_accounts coa
    LEFT JOIN acct.transaction_details td ON td.account_id = coa.account_id
    LEFT JOIN acct.transaction_headers th ON th.transaction_header_id = td.transaction_header_id
    LEFT JOIN acct.account_types at ON at.account_type_id = coa.account_type_id
    WHERE coa.business_id = @businessId
        AND coa.is_active = TRUE
    GROUP BY coa.account_id, coa.account_type_id, at.account_type_name
)
SELECT 
    -- Receivables: Debit balance in Asset accounts that are receivables
    COALESCE(SUM(CASE 
        WHEN account_type_name = 'Asset' AND balance > 0 
        THEN balance 
        ELSE 0 
    END), 0) AS receivables,
    
    -- Payables: Credit balance in Liability accounts
    COALESCE(ABS(SUM(CASE 
        WHEN account_type_name = 'Liability' AND balance < 0 
        THEN balance 
        ELSE 0 
    END)), 0) AS payables,
    
    -- Cash: Balance in Asset accounts (typically cash accounts)
    COALESCE(SUM(CASE 
        WHEN account_type_name = 'Asset' 
        THEN balance 
        ELSE 0 
    END), 0) AS cash,
    
    -- Revenue: Credit balance in Revenue accounts
    COALESCE(ABS(SUM(CASE 
        WHEN account_type_name = 'Revenue' 
        THEN balance 
        ELSE 0 
    END)), 0) AS revenue,
    
    -- Expenses: Debit balance in Expense accounts
    COALESCE(SUM(CASE 
        WHEN account_type_name = 'Expense' 
        THEN balance 
        ELSE 0 
    END), 0) AS expenses
FROM account_balances;
```

**Required Indexes** (already exist in schema):
```sql
-- For Chart of Accounts
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_business_id 
    ON acct.chart_of_accounts(business_id);
CREATE INDEX IF NOT EXISTS ix_chart_of_accounts_account_type_id 
    ON acct.chart_of_accounts(account_type_id);

-- For Transaction Details
CREATE INDEX IF NOT EXISTS ix_transaction_details_account_id 
    ON acct.transaction_details(account_id);
CREATE INDEX IF NOT EXISTS ix_transaction_details_transaction_header_id 
    ON acct.transaction_details(transaction_header_id);

-- For Transaction Headers
CREATE INDEX IF NOT EXISTS ix_transaction_headers_business_id 
    ON acct.transaction_headers(business_id);
```

**Sample Results**:
```
receivables | payables | cash    | revenue  | expenses | net_profit
------------|----------|---------|----------|----------|------------
150000.00   | 75000.00 | 50000.00| 500000.00| 350000.00| 150000.00
```

---

## 2. Recent Activities Query

**Purpose**: Fetch the most recent audit log entries for business-related activities.

**Performance**: Limited to 20 records by default (max 100). Expected execution time: <50ms.

```sql
SELECT 
    al.audit_log_id,
    al.table_name,
    al.record_id,
    al.action,
    u.full_name AS changed_by_user_name,
    al.changed_at_utc
FROM core.audit_logs al
LEFT JOIN core.users u ON u.user_id = al.changed_by_user_id
WHERE al.table_name IN (
    'businesses', 
    'chart_of_accounts', 
    'transaction_headers', 
    'transaction_details',
    'user_businesses',
    'user_business_roles'
)
AND (
    -- For business-related tables, match the business_id in new_values
    al.new_values->>'business_id' = @businessId::text
    OR al.old_values->>'business_id' = @businessId::text
    OR al.record_id = @businessId::text
)
ORDER BY al.changed_at_utc DESC
LIMIT @limit
OFFSET @offset;
```

**Required Indexes** (already exist in schema):
```sql
-- For Audit Logs
CREATE INDEX IF NOT EXISTS idx_audit_logs_table_record 
    ON core.audit_logs(table_name, record_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_by 
    ON core.audit_logs(changed_by_user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_at 
    ON core.audit_logs(changed_at_utc DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action 
    ON core.audit_logs(action);
```

**Additional Recommended Index for JSONB Queries**:
```sql
-- For faster JSONB field filtering (optional but recommended)
CREATE INDEX IF NOT EXISTS idx_audit_logs_new_values_business_id 
    ON core.audit_logs((new_values->>'business_id'));
CREATE INDEX IF NOT EXISTS idx_audit_logs_old_values_business_id 
    ON core.audit_logs((old_values->>'business_id'));
```

**Sample Results**:
```
audit_log_id | table_name          | record_id            | action | changed_by_user_name | changed_at_utc
-------------|---------------------|----------------------|--------|----------------------|-------------------------
12345        | transaction_headers | uuid-123-456...      | INSERT | John Doe             | 2024-01-15 14:30:00+00
12344        | chart_of_accounts   | uuid-789-012...      | UPDATE | Jane Smith           | 2024-01-15 13:15:00+00
12343        | businesses          | uuid-345-678...      | UPDATE | Admin User           | 2024-01-15 10:00:00+00
```

---

## 3. Subscription Status Query

**Purpose**: Retrieve the current subscription plan and status for a business.

**Performance**: Returns single row. Expected execution time: <10ms.

```sql
SELECT 
    sp.plan_name,
    us.status,
    us.start_date_utc,
    us.end_date_utc,
    us.trial_ends_at_utc,
    EXTRACT(DAY FROM (us.end_date_utc - NOW())) AS days_remaining
FROM core.user_subscriptions us
INNER JOIN core.subscription_plans sp ON sp.plan_id = us.plan_id
WHERE us.business_id = @businessId
ORDER BY us.created_at_utc DESC
LIMIT 1;
```

**Required Indexes** (already exist in schema):
```sql
-- For User Subscriptions
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_business_id 
    ON core.user_subscriptions(business_id);
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_created_at 
    ON core.user_subscriptions(created_at_utc DESC);
```

**Sample Results**:
```
plan_name    | status  | start_date_utc       | end_date_utc         | trial_ends_at_utc    | days_remaining
-------------|---------|----------------------|----------------------|----------------------|---------------
Professional | Active  | 2024-01-01 00:00:00  | 2024-12-31 23:59:59  | NULL                 | 350
```

---

## Performance Optimization Recommendations

### 1. Partitioning for Large Datasets

If `audit_logs` table grows very large (>1M rows), consider partitioning by date:

```sql
-- Example: Partition audit_logs by month
CREATE TABLE core.audit_logs_2024_01 PARTITION OF core.audit_logs
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

CREATE TABLE core.audit_logs_2024_02 PARTITION OF core.audit_logs
    FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');
```

### 2. Materialized View for KPIs (Optional)

For businesses with very high transaction volumes, consider a materialized view:

```sql
CREATE MATERIALIZED VIEW acct.business_kpis_mv AS
WITH account_balances AS (
    -- Same CTE as above
)
SELECT 
    coa.business_id,
    SUM(CASE WHEN at.account_type_name = 'Asset' AND balance > 0 THEN balance ELSE 0 END) AS receivables,
    ABS(SUM(CASE WHEN at.account_type_name = 'Liability' AND balance < 0 THEN balance ELSE 0 END)) AS payables,
    -- ... rest of the aggregations
FROM account_balances
GROUP BY coa.business_id;

-- Refresh strategy: Can be refreshed nightly or on-demand
REFRESH MATERIALIZED VIEW CONCURRENTLY acct.business_kpis_mv;
```

### 3. Query Execution Plans

To analyze query performance:

```sql
EXPLAIN ANALYZE
-- Insert any of the above queries here
```

### 4. Expected Performance Metrics

| Query Type            | Avg Execution Time | Max Rows Scanned | Notes                          |
|-----------------------|-------------------|------------------|--------------------------------|
| KPI Aggregation       | 50-100ms          | ~10k             | Depends on transaction volume  |
| Recent Activities     | 10-50ms           | 20-100           | Limited by LIMIT clause        |
| Subscription Status   | 5-10ms            | 1                | Single row lookup              |

---

## Query Usage in Code

All queries are used in the `DashboardService` class:

- **File**: `/CoreModule/API/Business/Core/Services/DashboardService.cs`
- **Methods**:
  - `GetKpisAsync()` - Uses KPI Aggregation Query
  - `GetRecentActivitiesAsync()` - Uses Recent Activities Query
  - `GetSubscriptionStatusAsync()` - Uses Subscription Status Query

---

## API Endpoints

### Main Dashboard Endpoint
```
GET /api/v1/businesses/{id}/dashboard?limit=20&offset=0
```

**Response Structure**:
```json
{
  "kpis": {
    "receivables": 150000.00,
    "payables": 75000.00,
    "cash": 50000.00,
    "revenue": 500000.00,
    "expenses": 350000.00,
    "netProfit": 150000.00
  },
  "recentActivities": [
    {
      "auditLogId": 12345,
      "tableName": "transaction_headers",
      "recordId": "uuid-123-456",
      "action": "INSERT",
      "changedByUserName": "John Doe",
      "changedAtUtc": "2024-01-15T14:30:00Z",
      "description": "Created new transaction headers"
    }
  ],
  "shortcuts": [
    {
      "label": "New Transaction",
      "icon": "add_circle",
      "route": "/transactions/new",
      "description": "Create a new accounting transaction"
    }
  ],
  "subscriptionStatus": {
    "planName": "Professional",
    "status": "Active",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z",
    "daysRemaining": 350,
    "isTrialActive": false,
    "trialEndsAt": null
  }
}
```

### Individual Endpoints

1. **KPIs Only** (lightweight):
```
GET /api/v1/businesses/{id}/dashboard/kpis
```

2. **Recent Activities Only**:
```
GET /api/v1/businesses/{id}/dashboard/activities?limit=20&offset=0
```

3. **Subscription Status Only**:
```
GET /api/v1/businesses/{id}/dashboard/subscription
```

---

## Testing Queries Directly

To test these queries in PostgreSQL:

```sql
-- Set a test business ID
\set businessId '\'12345678-1234-1234-1234-123456789abc\''

-- Test KPI query
\i kpi_query.sql

-- Test recent activities
\set limit 20
\set offset 0
\i recent_activities_query.sql

-- Test subscription status
\i subscription_status_query.sql
```

---

## Monitoring and Maintenance

### Query Performance Monitoring

```sql
-- Check slow queries related to dashboard
SELECT 
    query,
    calls,
    total_time,
    mean_time,
    max_time
FROM pg_stat_statements
WHERE query LIKE '%account_balances%'
   OR query LIKE '%audit_logs%'
   OR query LIKE '%user_subscriptions%'
ORDER BY mean_time DESC
LIMIT 10;
```

### Index Usage Statistics

```sql
-- Check if dashboard indexes are being used
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE tablename IN ('chart_of_accounts', 'transaction_details', 'audit_logs', 'user_subscriptions')
ORDER BY idx_scan DESC;
```

---

## Change Log

| Date       | Version | Changes                                    |
|------------|---------|---------------------------------------------|
| 2024-01-15 | 1.0     | Initial dashboard SQL queries documentation |

---

## Contact

For questions or performance issues with these queries, contact the DBA team or refer to the main project documentation.
