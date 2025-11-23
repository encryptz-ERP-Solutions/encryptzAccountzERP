# API Changelog: Core Schema Improvements Support

## Version: 1.0
## Date: 2025-03-XX

## Overview
Backend API updates to support core schema improvements including audit tracking, user-business relationships, subscription status enum, and audit logging.

---

## New Features

### 1. Audit Tracking
- Added audit fields to all key entities: `created_by_user_id`, `updated_by_user_id`, `created_at_utc`, `updated_at_utc`
- Server automatically populates these fields on create/update operations
- Fields are nullable to support backward compatibility

**Affected Tables:**
- `core.roles`
- `core.permissions`
- `core.modules`
- `core.menu_items`
- `core.subscription_plans`
- `core.businesses`

### 2. User-Business Management
- New `core.user_businesses` table for mapping users to businesses
- Support for default business selection (one per user)
- New endpoints:
  - `GET /api/user-businesses?userId={userId}` - List user's business links
  - `POST /api/user-businesses` - Create user-business link
  - `POST /api/user-businesses/{id}/set-default` - Set default business
  - `DELETE /api/user-businesses/{id}` - Remove user-business link
  - `GET /api/users/{userId}/default-business` - Get user's default business

### 3. Subscription Status Enum
- Created `core.subscription_status` enum type
- Converted `core.user_subscriptions.status` from VARCHAR to enum
- C# enum `SubscriptionStatus` added to DTOs

**Enum Values:**
- `active`
- `inactive`
- `suspended`
- `cancelled`
- `expired`
- `trial`
- `pending`
- `past_due`

### 4. Audit Logging
- New `core.audit_logs` table for tracking changes
- Automatic audit log entries on create/update/delete operations
- Best-effort logging (won't fail main operation if audit fails)

**Logged Operations:**
- Role create/update/delete
- Permission create/update/delete
- Module create/update/delete
- MenuItem create/update/delete
- SubscriptionPlan create/update/delete
- UserBusiness create/update/delete

---

## Infrastructure Changes

### New Components

1. **NpgsqlConnectionFactory**
   - Factory for creating PostgreSQL connections
   - Replaces SQL Server-specific connection handling

2. **AuthHelper**
   - Utility for extracting current user ID from JWT claims
   - Checks `user_id` claim first, falls back to `sub`

3. **AuditRepository & AuditService**
   - Handles writing audit log entries
   - Best-effort implementation (errors are logged but don't fail operations)

4. **UserBusinessRepository & UserBusinessService**
   - Manages user-business relationships
   - Transactional support for set-default operations

### Updated Components

1. **DTOs**
   - All response DTOs now include audit fields
   - New DTOs: `UserBusinessDto`, `CreateUserBusinessRequest`
   - New enum: `SubscriptionStatus`

2. **Repositories**
   - Updated to use PostgreSQL (Npgsql) instead of SQL Server (SqlClient)
   - Method signatures updated to accept `createdByUserId`/`updatedByUserId`
   - Queries updated to use snake_case column names
   - Queries include audit fields in SELECT statements

3. **Services**
   - Method signatures updated to accept audit user IDs
   - Pass audit user IDs to repositories

4. **Controllers**
   - Inject `IAuditService` for audit logging
   - Use `AuthHelper.GetCurrentUserId()` to get current user
   - Call `_auditService.LogAsync()` after mutations
   - Return audit fields in responses

---

## Breaking Changes

### Database
- **Migration Required**: Must run `0002_core_api_support.sql` before deploying API changes
- **Column Names**: PostgreSQL uses snake_case (e.g., `role_id` not `RoleID`)
- **Enum Type**: `user_subscriptions.status` is now an enum type

### API
- **Response DTOs**: Now include audit fields (backward compatible as they're nullable)
- **Repository Interfaces**: Method signatures changed to include audit user IDs
- **Service Interfaces**: Method signatures changed to include audit user IDs

### Code
- **Connection Handling**: Must use `IDbConnectionFactory` instead of `CoreSQLDbHelper`
- **Database Driver**: Must use Npgsql instead of SqlClient

---

## Migration Path

### For Existing Code

1. **Update Connection Handling**:
   ```csharp
   // Before
   private readonly CoreSQLDbHelper _sqlHelper;
   
   // After
   private readonly IDbConnectionFactory _connectionFactory;
   ```

2. **Update Repository Methods**:
   ```csharp
   // Before
   Task<Role> AddAsync(Role role);
   
   // After
   Task<Role> AddAsync(Role role, Guid? createdByUserId);
   ```

3. **Update Service Methods**:
   ```csharp
   // Before
   Task<RoleDto> AddRoleAsync(RoleDto roleDto);
   
   // After
   Task<RoleDto> AddRoleAsync(RoleDto roleDto, Guid? createdByUserId);
   ```

4. **Update Controllers**:
   ```csharp
   // Add audit service injection
   private readonly IAuditService _auditService;
   
   // Get current user ID
   var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
   
   // Call service with user ID
   var result = await _roleService.AddRoleAsync(roleDto, currentUserId);
   
   // Log audit
   await _auditService.LogAsync(currentUserId, "INSERT", "roles", result.RoleID.ToString());
   ```

---

## Testing

### Unit Tests
- Test `AuthHelper.GetCurrentUserId()` with various claim scenarios
- Test audit logging (mock repository)
- Test user-business set-default transaction logic

### Integration Tests
- Test role create/update with audit fields
- Test user-business create with isDefault=true
- Test set-default ensures only one default per user
- Test audit log entries are created

### Manual Testing
See `POSTMAN_TESTS.md` for complete test collection.

---

## Security Considerations

1. **Audit Fields**: Server-populated, never accept from client
2. **User ID Extraction**: Uses JWT claims, falls back gracefully if not authenticated
3. **Transactions**: Used for critical operations (set-default)
4. **Best-Effort Audit**: Audit failures don't expose errors to clients

---

## Performance Considerations

1. **Indexes**: Added on foreign keys and audit columns for query performance
2. **Partial Indexes**: Used for audit columns (only where NOT NULL)
3. **Async Operations**: All database operations are async
4. **Connection Pooling**: Npgsql handles connection pooling automatically

---

## Known Limitations

1. **Audit Logging**: Best-effort only (failures are logged but don't fail operations)
2. **Enum Conversion**: Invalid status values are converted to 'inactive' during migration
3. **Backward Compatibility**: Audit fields are nullable to support existing data

---

## Future Enhancements

1. Add audit log query endpoint
2. Add audit log filtering and pagination
3. Add subscription status validation in API
4. Add unit tests for all new components
5. Add integration tests for critical flows
6. Consider adding audit log retention policy

---

## Files Changed

### New Files
- `Infrastructure/NpgsqlConnectionFactory.cs`
- `Infrastructure/AuthHelper.cs`
- `Repository/Core/AuditRepository.cs`
- `Repository/Core/UserBusinessRepository.cs`
- `Business/Core/Services/AuditService.cs`
- `Business/Core/Services/UserBusinessService.cs`
- `Business/Core/DTOs/UserBusinessDto.cs`
- `encryptzERP/Controllers/Core/UserBusinessesController.cs`
- `encryptzERP/Controllers/Core/UsersController.cs`

### Updated Files
- `Business/Core/DTOs/RoleDto.cs` (added audit fields)
- `Business/Core/DTOs/PermissionDto.cs` (added audit fields)
- `Business/Core/DTOs/ModuleDto.cs` (added audit fields)
- `Business/Core/DTOs/MenuItemDto.cs` (added audit fields)
- `Business/Core/DTOs/SubscriptionPlanDto.cs` (added audit fields and enum)
- `Business/Core/DTOs/BusinessDto.cs` (added audit fields)

### Example Files (for reference)
- `encryptzERP/Controllers/Core/RolesController_Updated.cs.example`
- `Repository/Core/RoleRepository_Updated.cs.example`
- `Business/Core/Services/RoleService_Updated.cs.example`

---

## Dependencies

- **Npgsql**: PostgreSQL .NET driver
- **Microsoft.Extensions.Logging**: For logging support

---

## Documentation

- `API_MIGRATION_README.md` - Step-by-step migration guide
- `POSTMAN_TESTS.md` - Postman test collection
- `0002_core_api_support.sql` - Database migration script

