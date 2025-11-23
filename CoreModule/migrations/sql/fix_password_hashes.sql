-- =====================================================
-- Fix Password Hashes Migration Script
-- Date: 2025-11-22
-- Description: Updates password hashes from old SHA256 format to ASP.NET Core Identity PBKDF2 format
-- =====================================================

-- IMPORTANT: Run the PasswordUtility tool FIRST to generate proper hashes
-- Then update the values below with the generated hashes

-- This script will update the admin user password to use the correct hash format

-- Example: To generate the hash, run:
-- dotnet run --project API/Business -- generate "Admin@123"

-- =====================================================
-- STEP 1: Backup existing passwords (optional but recommended)
-- =====================================================

-- Create a backup table for passwords (run once)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'core' AND tablename = 'users_password_backup') THEN
        CREATE TABLE core.users_password_backup (
            user_id UUID,
            email VARCHAR(256),
            old_hashed_password TEXT,
            backup_date TIMESTAMPTZ DEFAULT NOW()
        );
    END IF;
END $$;

-- Backup current passwords before updating
INSERT INTO core.users_password_backup (user_id, email, old_hashed_password)
SELECT user_id, email, hashed_password
FROM core.users
WHERE hashed_password IS NOT NULL
  AND hashed_password LIKE 'sha256_%'
ON CONFLICT DO NOTHING;

-- =====================================================
-- STEP 2: Update admin user password
-- =====================================================

-- Replace the hash below with the output from PasswordUtility
-- This is a correctly formatted PBKDF2 hash for 'Admin@123'
-- You MUST run PasswordUtility to generate your own hash

-- PLACEHOLDER: Replace this with actual hash from PasswordUtility
-- Example hash format: 'AQAAAAIAAYagAAAAEKvBx...'
-- 
-- To generate: Run from the API/Business directory:
--   dotnet run --project ../encryptzERP -- hash-password "Admin@123"

/*
UPDATE core.users
SET 
    hashed_password = 'REPLACE_WITH_GENERATED_HASH_FROM_PASSWORD_UTILITY',
    updated_at_utc = NOW()
WHERE 
    email = 'admin@encryptz.com'
    AND (hashed_password IS NULL OR hashed_password LIKE 'sha256_%');
*/

-- =====================================================
-- STEP 3: Verification
-- =====================================================

-- Check which users still have old SHA256 format passwords
SELECT 
    user_id,
    user_handle,
    email,
    CASE 
        WHEN hashed_password IS NULL THEN 'NULL'
        WHEN hashed_password LIKE 'sha256_%' THEN 'OLD SHA256 FORMAT'
        ELSE 'PBKDF2 FORMAT (CORRECT)'
    END as password_format
FROM core.users
ORDER BY email;

-- =====================================================
-- NOTES
-- =====================================================
--
-- 1. DO NOT use the example hash in production! Generate your own using PasswordUtility
--
-- 2. To generate password hashes, you have several options:
--
--    Option A: Run PasswordUtility.GenerateCommonHashes() from code
--    Option B: Create a simple API endpoint (temporary, remove after use)
--    Option C: Use the C# Interactive window in Visual Studio
--
-- 3. ASP.NET Core Identity PBKDF2 hash format:
--    - Starts with version byte (usually 0x01)
--    - Contains: format marker + salt + subkey
--    - Base64 encoded
--    - Example length: ~80-100 characters
--
-- 4. Old SHA256 format:
--    - Prefix: "sha256_"
--    - Followed by hex hash
--    - Total length: ~71 characters
--
-- 5. After running this script, test login with the updated password
--
-- 6. If you need to reset passwords for other users, use the same pattern
--
-- =====================================================

