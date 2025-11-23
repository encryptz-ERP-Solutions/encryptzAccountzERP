# Login Error Fix - Base-64 Decoding Issue

## ❌ Error
```
The input is not a valid Base-64 string as it contains a non-base 64 character, 
more than two padding characters, or an illegal character among the padding characters.
```

## 🔍 Root Cause Analysis

### The Problem
Your database contains passwords hashed in **old SHA256 format**, but your application code uses **ASP.NET Core Identity's PasswordHasher** which expects **PBKDF2 Base-64 format**.

### Where It Fails
```
AuthController (/api/v1/auth/login)
  ↓
AuthService.LoginAsync() [Line 78-111]
  ↓
PasswordHasher.VerifyPassword() [Line 104]
  ↓
ASP.NET Identity PasswordHasher tries to decode Base-64
  ↓
❌ ERROR: Can't decode "sha256_e86f78..." as Base-64
```

### Database Format (WRONG):
```sql
-- Current format in database:
hashed_password = 'sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7'
```

### Expected Format (CORRECT):
```sql
-- Expected format by PasswordHasher:
hashed_password = 'AQAAAAIAAYagAAAAEKvBx7P2m9jKZ3F8rN6tL5X9pM3BfQy2Hj8D...'
```

---

## ✅ Quick Fix (Choose One)

### Option 1: Use API Endpoint (Fastest)

#### Step 1: Start your API
```bash
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/encryptzERP
dotnet run
```

#### Step 2: Generate correct hash
```bash
# Generate hash for "Admin@123"
curl -X POST "http://localhost:5286/api/PasswordHash/generate" \
  -H "Content-Type: application/json" \
  -d '{"password": "Admin@123"}'
```

**Example Response:**
```json
{
  "password": "Admin@123",
  "hash": "AQAAAAIAAYagAAAAEJ8vK3mN2pL...",
  "hashLength": 88,
  "verificationTest": "PASSED"
}
```

#### Step 3: Update database
Copy the `hash` value and run:

```sql
-- Connect to your PostgreSQL database
UPDATE core.users
SET 
    hashed_password = 'PASTE_THE_HASH_HERE',
    updated_at_utc = NOW()
WHERE 
    email = 'admin@encryptz.com';
```

#### Step 4: Test login
```bash
curl -X POST "http://localhost:5286/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUserHandle": "admin@encryptz.com",
    "password": "Admin@123"
  }'
```

#### Step 5: Remove temporary endpoint ⚠️
**IMPORTANT:** Delete this file before deploying to production:
```bash
rm /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/encryptzERP/Controllers/Admin/PasswordHashController.cs
```

---

### Option 2: Use C# Script

Create a temporary file `GenerateHash.csx` and run with `dotnet-script`:

```csharp
#r "nuget: Microsoft.AspNetCore.Identity, 2.2.0"
using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<object>();
var password = "Admin@123";
var hash = hasher.HashPassword(null, password);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Length: {hash.Length}");

// Verify it works
var result = hasher.VerifyHashedPassword(null, hash, password);
Console.WriteLine($"Verification: {result}");
```

Run:
```bash
dotnet script GenerateHash.csx
```

Then use the generated hash in Step 3 above.

---

### Option 3: Direct Database Fix (If you know the user exists)

If you want to create a completely new password:

```sql
-- First, backup the old password
INSERT INTO core.users_password_backup (user_id, email, old_hashed_password)
SELECT user_id, email, hashed_password
FROM core.users
WHERE email = 'admin@encryptz.com';

-- Then, temporarily set it to NULL
UPDATE core.users
SET hashed_password = NULL
WHERE email = 'admin@encryptz.com';

-- Use password reset flow via API
-- POST /api/v1/auth/forgot-password
-- This will send an OTP

-- Then reset via OTP
-- POST /api/v1/auth/reset-password
```

---

## 🔧 Files Created to Help You

I've created the following files:

1. **`PasswordHashController.cs`**
   - Temporary API endpoint to generate password hashes
   - Endpoints: `/api/PasswordHash/generate`, `/api/PasswordHash/verify`, `/api/PasswordHash/batch`
   - ⚠️ **DELETE BEFORE PRODUCTION!**

2. **`PasswordUtility.cs`**
   - Utility class for generating/verifying password hashes
   - Can be used for future migrations or testing

3. **`fix_password_hashes.sql`**
   - SQL migration script with instructions
   - Includes backup strategy

4. **`PASSWORD_HASH_FIX_GUIDE.md`**
   - Detailed guide with multiple fix options

5. **`LOGIN_ERROR_FIX.md`** (this file)
   - Quick reference for the specific error

---

## 🔒 Long-Term Fix

### Update Seed Data Files

Update `/migrations/sql/2025_11_20_sample_data.sql` line 38:

```sql
-- BEFORE (WRONG):
password_hash = 'sha256_e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7'

-- AFTER (CORRECT): Use hash generated from PasswordHasher
password_hash = 'AQAAAAIAAYagAAAA...' -- Generate using PasswordHashController or PasswordUtility
```

---

## 📊 Verify the Fix

### Check Password Format
```sql
SELECT 
    user_id,
    user_handle,
    email,
    CASE 
        WHEN hashed_password IS NULL THEN '⚠️ NULL'
        WHEN hashed_password LIKE 'sha256_%' THEN '❌ OLD SHA256'
        WHEN LENGTH(hashed_password) > 70 THEN '✅ PBKDF2 (CORRECT)'
        ELSE '❓ UNKNOWN'
    END as password_format,
    LENGTH(hashed_password) as hash_length,
    LEFT(hashed_password, 20) as hash_preview
FROM core.users
ORDER BY email;
```

Expected output:
```
| email               | password_format      | hash_length | hash_preview          |
|---------------------|---------------------|-------------|-----------------------|
| admin@encryptz.com  | ✅ PBKDF2 (CORRECT) | 88          | AQAAAAIAAYagAAAA... |
```

### Test Login
```bash
# Should return access token and user info
curl -X POST "http://localhost:5286/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUserHandle": "admin@encryptz.com",
    "password": "Admin@123"
  }' | jq
```

Expected response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-23T10:30:00Z",
  "userId": "33333333-3333-3333-3333-000000000001",
  "userHandle": "admin"
}
```

---

## 🛡️ Security Best Practices

1. ✅ Always use `PasswordHasher.HashPassword()` for new passwords
2. ✅ Never store plain text or custom hash formats
3. ✅ Use strong passwords (min 8 chars, uppercase, lowercase, number, special char)
4. ✅ Change default passwords immediately
5. ⚠️ Remove `PasswordHashController.cs` before production
6. ✅ Keep ASP.NET Core Identity libraries updated
7. ✅ Use HTTPS in production
8. ✅ Implement account lockout after failed attempts
9. ✅ Enable multi-factor authentication (MFA)
10. ✅ Regularly audit user passwords

---

## 📝 Technical Details

### PBKDF2 Hash Format (ASP.NET Core Identity)
- **Algorithm**: PBKDF2 with HMAC-SHA256
- **Iterations**: 10,000 (default)
- **Salt**: 128-bit (randomly generated per password)
- **Subkey**: 256-bit
- **Encoding**: Base-64
- **Length**: Typically 80-100 characters
- **Format**: `<version byte><salt><subkey>` all Base-64 encoded

### Old SHA256 Hash Format (Your Database)
- **Algorithm**: SHA256
- **Format**: `sha256_<hex_hash>`
- **Length**: 71 characters
- **Salt**: None (insecure!)
- **Iterations**: 1 (vulnerable to brute force)

---

## 🆘 Troubleshooting

### If you still get the error after updating:

1. **Clear connection pools**:
   ```bash
   # Restart your API
   pkill -f dotnet
   cd API/encryptzERP && dotnet run
   ```

2. **Verify database connection**:
   ```bash
   # Check connection string in appsettings.json
   cat API/encryptzERP/appsettings.Development.json | grep ConnectionString
   ```

3. **Check user exists**:
   ```sql
   SELECT * FROM core.users WHERE email = 'admin@encryptz.com';
   ```

4. **View full error stack trace**:
   - Check console output where API is running
   - Check logs in `API/encryptzERP/logs/` if configured

5. **Test password hasher independently**:
   ```bash
   curl -X POST "http://localhost:5286/api/PasswordHash/verify" \
     -H "Content-Type: application/json" \
     -d '{
       "password": "Admin@123",
       "hash": "YOUR_HASH_FROM_DATABASE"
     }'
   ```

---

## 📞 Need More Help?

If the issue persists:
1. Check that `PasswordHasher` is using `Microsoft.AspNetCore.Identity`
2. Verify no custom authentication middleware is interfering
3. Check for any database triggers modifying passwords
4. Ensure connection string points to the correct database
5. Review application logs for the full exception stack trace

---

**Created**: 2025-11-22  
**Status**: Ready to implement  
**Priority**: HIGH - Blocking login functionality

