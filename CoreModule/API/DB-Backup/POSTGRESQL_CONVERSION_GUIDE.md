# PostgreSQL Conversion Guide

This document provides patterns for converting remaining SQL Server code to PostgreSQL.

## Conversion Patterns

### 1. Using Statements
**Replace:**
```csharp
using Microsoft.Data.SqlClient;
```

**With:**
```csharp
using Npgsql;
```

### 2. Parameter Types
**Replace:**
```csharp
new SqlParameter("@ParamName", value)
```

**With:**
```csharp
new NpgsqlParameter("@ParamName", value)
```

### 3. SQL Syntax Conversions

#### Column Names (PascalCase → snake_case)
- `BusinessID` → `business_id`
- `BusinessName` → `business_name`
- `CreatedAtUTC` → `created_at_utc`
- `IsActive` → `is_active`
- `UserID` → `user_id`
- `RoleID` → `role_id`
- `PermissionID` → `permission_id`
- `ModuleID` → `module_id`
- `MenuItemID` → `menu_item_id`
- `PlanID` → `plan_id`
- `SubscriptionID` → `subscription_id`

#### Table Names (PascalCase → lowercase)
- `core.Businesses` → `core.businesses`
- `core.Users` → `core.users`
- `core.Roles` → `core.roles`
- `core.Permissions` → `core.permissions`
- `core.Modules` → `core.modules`
- `core.MenuItems` → `core.menu_items`
- `core.SubscriptionPlans` → `core.subscription_plans`
- `core.UserSubscriptions` → `core.user_subscriptions`
- `core.RolePermissions` → `core.role_permissions`
- `core.UserBusinessRoles` → `core.user_business_roles`
- `core.SubscriptionPlanPermissions` → `core.subscription_plan_permissions`
- `Acct.TransactionHeaders` → `acct.transaction_headers`
- `Acct.TransactionDetails` → `acct.transaction_details`
- `Admin.OneTimePasswords` → `admin.one_time_passwords`

#### OUTPUT INSERTED.* → RETURNING
**SQL Server:**
```sql
INSERT INTO core.Roles (RoleName, Description)
OUTPUT INSERTED.*
VALUES (@RoleName, @Description);
```

**PostgreSQL:**
```sql
INSERT INTO core.roles (role_name, description)
VALUES (@RoleName, @Description)
RETURNING role_id, role_name, description;
```

#### UPDATE with SELECT → UPDATE with RETURNING
**SQL Server:**
```sql
UPDATE core.Users SET ...
WHERE UserID = @UserID;
SELECT * FROM core.Users WHERE UserID = @UserID;
```

**PostgreSQL:**
```sql
UPDATE core.users SET ...
WHERE user_id = @UserID
RETURNING user_id, user_handle, ...;
```

#### GETUTCDATE() / GETDATE() → NOW() AT TIME ZONE 'UTC'
**SQL Server:**
```sql
WHERE ExpiryTimeUTC > GETUTCDATE()
```

**PostgreSQL:**
```sql
WHERE expiry_time_utc > NOW() AT TIME ZONE 'UTC'
```

#### BIT → BOOLEAN
**SQL Server:**
```sql
IsActive BIT NOT NULL DEFAULT 1
```

**PostgreSQL:**
```sql
is_active BOOLEAN NOT NULL DEFAULT TRUE
```

#### ISNULL() → COALESCE()
**SQL Server:**
```sql
ISNULL(column, 'default')
```

**PostgreSQL:**
```sql
COALESCE(column, 'default')
```

### 4. DataRow Mapping
**Replace PascalCase column names with snake_case:**
```csharp
// Before
row["BusinessID"]
row["BusinessName"]
row["IsActive"]

// After
row["business_id"]
row["business_name"]
row["is_active"]
```

## Remaining Files to Convert

### Core Repositories
- [x] BusinessRepository.cs
- [x] RoleRepository.cs
- [x] UserRepository.cs (Admin)
- [x] LoginRepository.cs
- [x] TransactionRepository.cs
- [ ] PermissionRepository.cs
- [ ] ModuleRepository.cs
- [ ] MenuItemRepository.cs
- [ ] UserSubscriptionRepository.cs
- [ ] SubscriptionPlanRepository.cs
- [ ] SubscriptionPlanPermissionRepository.cs
- [ ] RolePermissionRepository.cs
- [ ] UserBusinessRoleRepository.cs
- [ ] ChartOfAccountRepository.cs
- [ ] AccountTypeRepository.cs

## Quick Conversion Script Pattern

For each repository file:

1. Replace `using Microsoft.Data.SqlClient;` with `using Npgsql;`
2. Replace all `SqlParameter` with `NpgsqlParameter`
3. Convert all SQL queries:
   - Table names: PascalCase → lowercase
   - Column names: PascalCase → snake_case
   - `OUTPUT INSERTED.*` → `RETURNING ...`
   - `GETUTCDATE()` → `NOW() AT TIME ZONE 'UTC'`
   - `BIT` → `BOOLEAN` (in queries, use TRUE/FALSE)
4. Update DataRow mappings to use snake_case column names

