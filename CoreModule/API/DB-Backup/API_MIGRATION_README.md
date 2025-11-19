# Backend API Migration Guide - Core Schema Improvements

## Overview
This guide covers updating the backend API to support the new core schema improvements including audit tracking, user-business relationships, subscription status enum, and audit logging.

## Prerequisites

1. **Database Migration Applied**
   ```bash
   psql -h localhost -U your_username -d encryptzERPCore -f 0002_core_api_support.sql
   ```

2. **NuGet Packages**
   - `Npgsql` (PostgreSQL .NET driver)
   - `Microsoft.Extensions.Logging` (if not already included)

   ```bash
   dotnet add package Npgsql
   ```

3. **Connection String**
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=encryptzERPCore;Username=your_user;Password=your_password"
     }
   }
   ```

## Step-by-Step Implementation

### 1. Update Program.cs

Add new service registrations:

```csharp
// Add after existing service registrations
builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserBusinessRepository, UserBusinessRepository>();
builder.Services.AddScoped<IUserBusinessService, UserBusinessService>();
```

**Note**: If you're still using `CoreSQLDbHelper` for SQL Server, you'll need to:
- Create a PostgreSQL version or
- Update `CoreSQLDbHelper` to use Npgsql instead of SqlClient
- Update all repositories to use `IDbConnectionFactory` instead of `CoreSQLDbHelper`

### 2. Update DTOs

All DTOs have been updated with audit fields:
- `RoleDto`
- `PermissionDto`
- `ModuleDto`
- `MenuItemDto`
- `SubscriptionPlanDto`
- `BusinessDto`

New DTOs created:
- `UserBusinessDto`
- `CreateUserBusinessRequest`
- `SubscriptionStatus` enum

### 3. Update Repositories

For each repository (Roles, Permissions, Modules, MenuItems, SubscriptionPlans, Businesses):

1. **Change from SqlClient to Npgsql**:
   - Replace `SqlConnection` with `NpgsqlConnection`
   - Replace `SqlParameter` with `NpgsqlParameter`
   - Replace `SqlCommand` with `NpgsqlCommand`
   - Use `IDbConnectionFactory` instead of `CoreSQLDbHelper`

2. **Update method signatures**:
   ```csharp
   // Before
   Task<Role> AddAsync(Role role);
   
   // After
   Task<Role> AddAsync(Role role, Guid? createdByUserId);
   ```

3. **Update INSERT queries**:
   ```sql
   INSERT INTO core.roles 
       (role_name, description, is_system_role, created_by_user_id, created_at_utc)
   VALUES 
       (@role_name, @description, @is_system_role, @created_by_user_id, NOW() AT TIME ZONE 'UTC')
   RETURNING role_id, role_name, description, is_system_role, created_by_user_id, created_at_utc, updated_by_user_id, updated_at_utc;
   ```

4. **Update UPDATE queries**:
   ```sql
   UPDATE core.roles
   SET 
       role_name = @role_name,
       updated_by_user_id = @updated_by_user_id,
       updated_at_utc = NOW() AT TIME ZONE 'UTC'
   WHERE role_id = @role_id;
   ```

5. **Update SELECT queries** to include audit fields:
   ```sql
   SELECT 
       role_id, role_name, description, is_system_role,
       created_by_user_id, created_at_utc, updated_by_user_id, updated_at_utc
   FROM core.roles;
   ```

6. **Update column name mappings**:
   - SQL Server: `RoleID` → PostgreSQL: `role_id`
   - SQL Server: `RoleName` → PostgreSQL: `role_name`
   - Use snake_case for all PostgreSQL column names

### 4. Update Services

For each service interface and implementation:

1. **Update interface**:
   ```csharp
   Task<RoleDto> AddRoleAsync(RoleDto roleDto, Guid? createdByUserId);
   Task<bool> UpdateRoleAsync(int id, RoleDto roleDto, Guid? updatedByUserId);
   ```

2. **Update implementation** to pass `createdByUserId`/`updatedByUserId` to repository

### 5. Update Controllers

For each controller (Roles, Permissions, Modules, MenuItems, SubscriptionPlans, Businesses):

1. **Inject `IAuditService`**:
   ```csharp
   private readonly IAuditService _auditService;
   ```

2. **Update POST endpoint**:
   ```csharp
   [HttpPost]
   public async Task<IActionResult> Create(RoleDto roleDto)
   {
       var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
       var newRole = await _roleService.AddRoleAsync(roleDto, currentUserId);
       
       // Audit log (best-effort)
       await _auditService.LogAsync(
           currentUserId,
           "INSERT",
           "roles",
           newRole.RoleID.ToString(),
           $"Created role: {newRole.RoleName}");
       
       return CreatedAtAction(nameof(GetById), new { id = newRole.RoleID }, newRole);
   }
   ```

3. **Update PUT endpoint**:
   ```csharp
   [HttpPut("{id}")]
   public async Task<IActionResult> Update(int id, RoleDto roleDto)
   {
       var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
       var success = await _roleService.UpdateRoleAsync(id, roleDto, currentUserId);
       
       if (!success) return NotFound();
       
       // Audit log
       await _auditService.LogAsync(
           currentUserId,
           "UPDATE",
           "roles",
           id.ToString(),
           $"Updated role: {roleDto.RoleName}");
       
       var updatedRole = await _roleService.GetRoleByIdAsync(id);
       return Ok(updatedRole);
   }
   ```

4. **Update DELETE endpoint**:
   ```csharp
   [HttpDelete("{id}")]
   public async Task<IActionResult> Delete(int id)
   {
       var role = await _roleService.GetRoleByIdAsync(id);
       if (role == null) return NotFound();
       
       var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
       var success = await _roleService.DeleteRoleAsync(id);
       
       if (!success) return NotFound();
       
       // Audit log
       await _auditService.LogAsync(
           currentUserId,
           "DELETE",
           "roles",
           id.ToString(),
           $"Deleted role: {role.RoleName}");
       
       return Ok(new { message = "Role deleted successfully." });
   }
   ```

### 6. New Controllers

**UserBusinessesController** - Already created, just register in Program.cs

**UsersController** - Add endpoint for default business (already created)

### 7. Update AutoMapper Profiles

Update mapping profiles to include audit fields:

```csharp
CreateMap<Role, RoleDto>()
    .ForMember(dest => dest.CreatedByUserID, opt => opt.MapFrom(src => src.CreatedByUserID))
    .ForMember(dest => dest.CreatedAtUTC, opt => opt.MapFrom(src => src.CreatedAtUTC))
    .ForMember(dest => dest.UpdatedByUserID, opt => opt.MapFrom(src => src.UpdatedByUserID))
    .ForMember(dest => dest.UpdatedAtUTC, opt => opt.MapFrom(src => src.UpdatedAtUTC));
```

Or if using convention-based mapping, ensure entity properties match DTO properties.

## Testing

### 1. Run Migration
```bash
psql -h localhost -U your_username -d encryptzERPCore -f 0002_core_api_support.sql
```

### 2. Verify Database
```sql
-- Check audit columns exist
SELECT column_name FROM information_schema.columns 
WHERE table_schema = 'core' AND table_name = 'roles' 
AND column_name IN ('created_by_user_id', 'updated_by_user_id', 'created_at_utc', 'updated_at_utc');

-- Check user_businesses table exists
SELECT * FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'user_businesses';

-- Check enum type
SELECT typname FROM pg_type WHERE typname = 'subscription_status';
```

### 3. Test API Endpoints

Use the Postman collection (see `POSTMAN_TESTS.md`):

1. **Create Role** - Verify `createdByUserID` and `createdAtUTC` are populated
2. **Update Role** - Verify `updatedByUserID` and `updatedAtUTC` are updated
3. **Create User-Business** - Verify link is created and audit log entry exists
4. **Set Default Business** - Verify only one default per user
5. **Check Audit Logs** - Verify entries are being written

### 4. Verify Audit Logging

```sql
SELECT * FROM core.audit_logs 
ORDER BY changed_at_utc DESC 
LIMIT 10;
```

## Common Issues & Solutions

### Issue: Column names not found
**Solution**: Ensure you're using PostgreSQL snake_case column names (`role_id` not `RoleID`)

### Issue: Enum type not found
**Solution**: Run migration to create `core.subscription_status` enum

### Issue: Foreign key constraint fails
**Solution**: Ensure `created_by_user_id` references valid `core.users(user_id)`

### Issue: Audit log not writing
**Solution**: Check logs for errors. Audit is best-effort and won't fail the main operation.

### Issue: Multiple defaults per user
**Solution**: The unique partial index should prevent this. Verify migration was applied correctly.

## Rollback

If you need to rollback:

1. **Database**: Uncomment rollback section in migration file and run
2. **Code**: Revert controller, service, and repository changes
3. **DTOs**: Remove audit fields (optional, they're nullable)

## Next Steps

1. Update remaining repositories (Permissions, Modules, MenuItems, SubscriptionPlans, Businesses)
2. Add unit tests for audit logging
3. Add integration tests for user-business operations
4. Consider adding audit log query endpoint
5. Add subscription status enum mapping in UserSubscriptions repository

## Support

For issues:
1. Check migration logs
2. Verify connection string
3. Check PostgreSQL logs
4. Review API logs for exceptions

