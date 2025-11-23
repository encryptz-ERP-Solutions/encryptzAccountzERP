# Password Hash Fix Guide

## Problem
The login API (`/api/v1/auth/login`) is throwing a Base-64 decoding error:
```
The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.
```

## Root Cause
The database contains passwords in **old SHA256 format** (`sha256_e86f78a...`), but the application code uses **ASP.NET Core Identity's PasswordHasher** which expects **PBKDF2 format** (Base-64 encoded).

## Quick Fix (Option 1: Use Temporary API Endpoint)

### Step 1: Start your API server
```bash
cd API/encryptzERP
dotnet run
```

### Step 2: Generate the correct password hash

Make a POST request to generate the hash for "Admin@123":

```bash
curl -X POST "http://localhost:5286/api/PasswordHash/generate" \
  -H "Content-Type: application/json" \
  -d '{"password": "Admin@123"}'
```

Or use the batch endpoint to generate multiple hashes:

```bash
curl -X GET "http://localhost:5286/api/PasswordHash/batch"
```

**Response Example:**
```json
{
  "password": "Admin@123",
  "hash": "AQAAAAIAAYagAAAAEKvBx7P2m9jKZ3F8rN6tL5X...",
  "hashLength": 88,
  "verificationTest": "PASSED",
  "format": "PBKDF2 (ASP.NET Core Identity)",
  "warning": "This hash is for database migration..."
}
```

### Step 3: Update the database

Copy the `hash` value from the response and run this SQL:

```sql
UPDATE core.users
SET 
    hashed_password = 'PASTE_HASH_HERE',
    updated_at_utc = NOW()
WHERE 
    email = 'admin@encryptz.com';
```

### Step 4: Test the login
```bash
curl -X POST "http://localhost:5286/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "loginIdentifier": "admin@encryptz.com",
    "password": "Admin@123"
  }'
```

### Step 5: Remove the temporary endpoint
After fixing the passwords, **delete** the file:
```
API/encryptzERP/Controllers/Admin/PasswordHashController.cs
```

This endpoint should NOT be deployed to production!

---

## Alternative Fix (Option 2: Using C# Interactive or Script)

### Step 1: Run the PasswordUtility

Create a small console app or use C# Interactive:

```csharp
using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<object>();
var hash = hasher.HashPassword(null, "Admin@123");
Console.WriteLine($"Hash: {hash}");

// Verify it works
var result = hasher.VerifyHashedPassword(null, hash, "Admin@123");
Console.WriteLine($"Verification: {result}"); // Should show "Success"
```

### Step 2: Update database with the generated hash

Use the SQL from Step 3 above.

---

## Alternative Fix (Option 3: Create a New User)

If you don't need the existing admin user, you can create a new one using the API:

1. Comment out authentication on the user creation endpoint (temporarily)
2. Create a new user via API
3. The new user will automatically get the correct hash format
4. Re-enable authentication

---

## Long-term Fix: Update All Seed Data

Update the sample data file to use correct hashes:

**File:** `migrations/sql/2025_11_20_sample_data.sql`

Replace line 38:
```sql
-- OLD (WRONG):
'sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7'

-- NEW (CORRECT):
'AQAAAAIAAYagAAAAEKvBx...' -- Use the hash generated from Step 2
```

---

## Verify the Fix

After updating the database, check the password format:

```sql
SELECT 
    user_id,
    email,
    CASE 
        WHEN hashed_password IS NULL THEN 'NULL'
        WHEN hashed_password LIKE 'sha256_%' THEN '❌ OLD SHA256 FORMAT'
        WHEN LENGTH(hashed_password) > 70 THEN '✅ PBKDF2 FORMAT (CORRECT)'
        ELSE '❓ UNKNOWN'
    END as password_format,
    LENGTH(hashed_password) as hash_length
FROM core.users
WHERE email = 'admin@encryptz.com';
```

Expected result:
- `password_format`: ✅ PBKDF2 FORMAT (CORRECT)
- `hash_length`: ~80-100 characters

---

## Understanding the Formats

### Old SHA256 Format (WRONG):
```
sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7
Length: 71 characters
```

### New PBKDF2 Format (CORRECT):
```
AQAAAAIAAYagAAAAEKvBx7P2m9jKZ3F8rN6tL5X9pM3BfQy2Hj8D...
Length: ~80-100 characters (Base-64 encoded)
Format: ASP.NET Core Identity PasswordHasher
Algorithm: PBKDF2 with HMAC-SHA256
Iterations: 10000
Salt: 128-bit
Subkey: 256-bit
```

---

## Prevention

To prevent this issue in the future:

1. ✅ Always use `PasswordHasher.HashPassword()` when creating/updating passwords
2. ✅ Never store passwords in plain text or custom hash formats
3. ✅ Test password hashing in seed scripts before running them
4. ✅ Use the `PasswordUtility` class for generating hashes
5. ✅ Validate hash format before inserting into database

---

## Need Help?

If you encounter issues:

1. Check that the database table exists: `SELECT * FROM core.users LIMIT 1;`
2. Check the password column: `SELECT email, hashed_password FROM core.users;`
3. Verify the PasswordHasher service is registered in DI
4. Check for any middleware interfering with authentication
5. Review the full error stack trace in the API logs

---

## Security Notes

⚠️ **IMPORTANT:**
- The PasswordHashController is for **development/migration only**
- **DELETE** it before deploying to production
- Change default passwords immediately
- Never expose password hashing endpoints in production
- Use strong passwords that meet security requirements

