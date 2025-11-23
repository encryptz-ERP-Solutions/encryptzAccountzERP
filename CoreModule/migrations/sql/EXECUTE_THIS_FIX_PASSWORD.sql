-- =====================================================
-- IMMEDIATE FIX FOR LOGIN BASE-64 ERROR
-- Date: 2025-11-22
-- Password: Admin@123
-- =====================================================

-- Backup the old password (optional but recommended)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'core' AND tablename = 'users_password_backup') THEN
        CREATE TABLE core.users_password_backup (
            backup_id SERIAL PRIMARY KEY,
            user_id UUID,
            email VARCHAR(256),
            old_hashed_password TEXT,
            backup_date TIMESTAMPTZ DEFAULT NOW()
        );
    END IF;
END $$;

-- Backup current password
INSERT INTO core.users_password_backup (user_id, email, old_hashed_password)
SELECT user_id, email, hashed_password
FROM core.users
WHERE email = 'abdu9744@gmail.com';

-- =====================================================
-- UPDATE PASSWORD TO CORRECT FORMAT
-- =====================================================

UPDATE core.users
SET hashed_password = 'AQAAAAEAACcQAAAAEFYKOtnjj2Yeo51sKMdUSCMvbUpHGGN5ttT4ZohIwmYLGxYwsYvIFlMWnRqNjm4QJA==',
    updated_at_utc = NOW()
WHERE email = 'abdu9744@gmail.com';

-- =====================================================
-- VERIFY THE UPDATE
-- =====================================================

SELECT 
    user_id,
    email,
    user_handle,
    CASE 
        WHEN hashed_password LIKE 'sha256_%' THEN '❌ OLD SHA256 FORMAT (WRONG)'
        WHEN LENGTH(hashed_password) > 70 THEN '✅ PBKDF2 FORMAT (CORRECT)'
        ELSE '❓ UNKNOWN FORMAT'
    END as password_status,
    LENGTH(hashed_password) as hash_length,
    is_active,
    updated_at_utc as last_updated
FROM core.users
WHERE email = 'abdu9744@gmail.com';

-- Expected output should show:
-- password_status: ✅ PBKDF2 FORMAT (CORRECT)
-- hash_length: 84
-- is_active: true

