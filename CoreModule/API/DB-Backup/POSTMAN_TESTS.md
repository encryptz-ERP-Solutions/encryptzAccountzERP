# Postman Test Collection for Core Schema Improvements

## Prerequisites
1. Ensure migration `0002_core_api_support.sql` has been applied
2. API is running and accessible
3. You have a valid JWT token (obtain via login endpoint)

## Base Configuration
- **Base URL**: `http://localhost:5000` (adjust as needed)
- **Authorization**: Bearer Token (set in Postman environment variable `{{token}}`)

---

## Test 1: Verify Migration Applied

### Request
```
GET {{baseUrl}}/api/health/check-migration
```

**Or use direct SQL query:**
```sql
SELECT COUNT(*) FROM core.user_businesses;
SELECT COUNT(*) FROM core.audit_logs;
SELECT typname FROM pg_type WHERE typname = 'subscription_status';
```

**Expected**: Tables exist, enum type exists

---

## Test 2: Create Role with Audit Fields

### Request
```
POST {{baseUrl}}/api/roles
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "roleName": "Test Role",
  "description": "Test role for audit verification",
  "isSystemRole": false
}
```

### Verification
1. Check response includes:
   - `createdByUserID` (should match your user ID from token)
   - `createdAtUTC` (should be current UTC timestamp)
   - `roleID` (newly created ID)

2. Query audit_logs:
```sql
SELECT * FROM core.audit_logs 
WHERE table_name = 'roles' 
ORDER BY changed_at_utc DESC 
LIMIT 1;
```

**Expected**: 
- Response contains audit fields
- Audit log entry exists with action='INSERT'

---

## Test 3: Update Role with Audit Fields

### Request
```
PUT {{baseUrl}}/api/roles/{{roleId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "roleID": {{roleId}},
  "roleName": "Updated Test Role",
  "description": "Updated description",
  "isSystemRole": false
}
```

### Verification
1. Check response includes:
   - `updatedByUserID` (should match your user ID)
   - `updatedAtUTC` (should be updated timestamp)
   - `createdByUserID` (should remain unchanged)

2. Query audit_logs:
```sql
SELECT * FROM core.audit_logs 
WHERE table_name = 'roles' AND action = 'UPDATE'
ORDER BY changed_at_utc DESC 
LIMIT 1;
```

**Expected**: 
- `updatedByUserID` and `updatedAtUTC` are populated
- Audit log entry exists with action='UPDATE'

---

## Test 4: Create User-Business Link

### Request
```
POST {{baseUrl}}/api/user-businesses
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userID": "{{userId}}",
  "businessID": "{{businessId}}",
  "isDefault": true
}
```

### Verification
1. Check response includes:
   - `userBusinessID` (newly created)
   - `isDefault` (should be true)
   - `createdAtUTC`

2. Verify only one default per user:
```sql
SELECT user_id, COUNT(*) as default_count
FROM core.user_businesses
WHERE user_id = '{{userId}}' AND is_default = TRUE
GROUP BY user_id;
```

**Expected**: `default_count = 1`

3. Check audit log:
```sql
SELECT * FROM core.audit_logs 
WHERE table_name = 'user_businesses' AND action = 'INSERT'
ORDER BY changed_at_utc DESC 
LIMIT 1;
```

---

## Test 5: Set Default Business (Transactional)

### Request
```
POST {{baseUrl}}/api/user-businesses/{{userBusinessId}}/set-default?userId={{userId}}
Authorization: Bearer {{token}}
```

### Verification
1. Verify previous default is unset:
```sql
SELECT user_business_id, is_default 
FROM core.user_businesses 
WHERE user_id = '{{userId}}'
ORDER BY is_default DESC, created_at_utc DESC;
```

**Expected**: Only one record has `is_default = TRUE`

2. Check audit log:
```sql
SELECT * FROM core.audit_logs 
WHERE table_name = 'user_businesses' AND action = 'UPDATE'
ORDER BY changed_at_utc DESC 
LIMIT 1;
```

---

## Test 6: Get User's Default Business

### Request
```
GET {{baseUrl}}/api/users/{{userId}}/default-business
Authorization: Bearer {{token}}
```

### Verification
**Expected Response**:
```json
{
  "businessId": "{{businessId}}"
}
```

Or `null` if no default is set.

---

## Test 7: List User-Business Links

### Request
```
GET {{baseUrl}}/api/user-businesses?userId={{userId}}
Authorization: Bearer {{token}}
```

### Verification
**Expected Response**: Array of UserBusinessDto objects, sorted by `isDefault` DESC, then `createdAtUTC` DESC

---

## Test 8: Delete User-Business Link

### Request
```
DELETE {{baseUrl}}/api/user-businesses/{{userBusinessId}}
Authorization: Bearer {{token}}
```

### Verification
1. Verify record is deleted:
```sql
SELECT * FROM core.user_businesses WHERE user_business_id = '{{userBusinessId}}';
```

**Expected**: No rows returned

2. Check audit log:
```sql
SELECT * FROM core.audit_logs 
WHERE table_name = 'user_businesses' AND action = 'DELETE'
ORDER BY changed_at_utc DESC 
LIMIT 1;
```

---

## Test 9: Verify Subscription Status Enum

### Request (if you have user_subscriptions endpoint)
```
GET {{baseUrl}}/api/user-subscriptions/{{subscriptionId}}
Authorization: Bearer {{token}}
```

### Verification
1. Check status field type:
```sql
SELECT column_name, data_type, udt_name 
FROM information_schema.columns 
WHERE table_schema = 'core' 
AND table_name = 'user_subscriptions' 
AND column_name = 'status';
```

**Expected**: `udt_name = 'subscription_status'`

2. Test enum values:
```sql
SELECT DISTINCT status FROM core.user_subscriptions;
```

**Expected**: Only valid enum values are present

---

## Test 10: Audit Log Query

### Direct SQL Query
```sql
-- Get recent audit logs
SELECT 
    audit_log_id,
    table_name,
    record_id,
    action,
    changed_by_user_id,
    changed_at_utc,
    new_values
FROM core.audit_logs
ORDER BY changed_at_utc DESC
LIMIT 10;
```

**Expected**: Recent audit entries for all mutations

---

## Postman Environment Variables

Create a Postman environment with:
- `baseUrl`: `http://localhost:5000`
- `token`: Your JWT token (obtained from login)
- `userId`: Test user ID (UUID)
- `businessId`: Test business ID (UUID)
- `roleId`: Test role ID (integer)
- `userBusinessId`: Test user-business link ID (UUID)
- `subscriptionId`: Test subscription ID (UUID)

---

## Error Scenarios to Test

1. **Create user-business with invalid user ID**: Should return 400
2. **Set default for non-existent user-business**: Should return 404
3. **Create role without authentication**: Should return 401
4. **Update role with mismatched ID**: Should return 400
5. **Delete non-existent user-business**: Should return 404

---

## Notes

- All audit operations are best-effort (won't fail the main operation)
- Audit fields are nullable (won't error if user is not authenticated)
- Transactions ensure data consistency for set-default operations
- Enum conversion handles invalid values gracefully (converts to 'inactive')

