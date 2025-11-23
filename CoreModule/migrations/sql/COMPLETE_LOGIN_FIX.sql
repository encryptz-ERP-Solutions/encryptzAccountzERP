-- =====================================================
-- COMPLETE LOGIN FIX
-- Fixes both issues:
-- 1. Password format (Base-64 error)
-- 2. Missing refresh_tokens table
-- =====================================================

-- =====================================================
-- PART 1: Create refresh_tokens table
-- =====================================================

-- Create the refresh_tokens table
CREATE TABLE IF NOT EXISTS core.refresh_tokens (
    refresh_token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at_utc TIMESTAMP NOT NULL,
    created_at_utc TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at_utc TIMESTAMP NULL,
    replaced_by_token_id UUID NULL,
    created_by_ip VARCHAR(45) NULL,
    revoked_by_ip VARCHAR(45) NULL,
    
    -- Foreign key constraints
    CONSTRAINT fk_refresh_tokens_user 
        FOREIGN KEY (user_id) 
        REFERENCES core.users(user_id) 
        ON DELETE CASCADE,
    
    CONSTRAINT fk_refresh_tokens_replaced_by 
        FOREIGN KEY (replaced_by_token_id) 
        REFERENCES core.refresh_tokens(refresh_token_id) 
        ON DELETE SET NULL,
    
    -- Constraints
    CONSTRAINT chk_expires_after_created 
        CHECK (expires_at_utc > created_at_utc),
    
    CONSTRAINT chk_revoked_consistency 
        CHECK ((is_revoked = FALSE AND revoked_at_utc IS NULL) OR 
               (is_revoked = TRUE AND revoked_at_utc IS NOT NULL))
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id 
    ON core.refresh_tokens(user_id);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token_hash 
    ON core.refresh_tokens(token_hash);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at 
    ON core.refresh_tokens(expires_at_utc);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_active 
    ON core.refresh_tokens(user_id, is_revoked, expires_at_utc) 
    WHERE is_revoked = FALSE;

-- Add comments
COMMENT ON TABLE core.refresh_tokens IS 'Stores hashed refresh tokens for JWT authentication with rotation support';

-- Cleanup function
CREATE OR REPLACE FUNCTION core.cleanup_expired_refresh_tokens()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM core.refresh_tokens
    WHERE expires_at_utc < (NOW() AT TIME ZONE 'UTC') 
       OR (is_revoked = TRUE AND revoked_at_utc < (NOW() AT TIME ZONE 'UTC') - INTERVAL '30 days');
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- PART 2: Backup and update password
-- =====================================================

-- Create backup table if not exists
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
WHERE email = 'abdu9744@gmail.com'
ON CONFLICT DO NOTHING;

-- Update password to correct PBKDF2 format
UPDATE core.users
SET hashed_password = 'AQAAAAEAACcQAAAAEFYKOtnjj2Yeo51sKMdUSCMvbUpHGGN5ttT4ZohIwmYLGxYwsYvIFlMWnRqNjm4QJA==',
    updated_at_utc = NOW()
WHERE email = 'abdu9744@gmail.com';

-- =====================================================
-- VERIFICATION
-- =====================================================

-- Check refresh_tokens table exists
SELECT 
    'refresh_tokens table' as check_item,
    CASE WHEN EXISTS (
        SELECT FROM pg_tables WHERE schemaname = 'core' AND tablename = 'refresh_tokens'
    ) THEN '✅ EXISTS' ELSE '❌ NOT FOUND' END as status;

-- Check password format
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

-- Show final status
SELECT 
    'LOGIN FIX STATUS' as summary,
    (SELECT COUNT(*) FROM core.refresh_tokens) as refresh_tokens_ready,
    (SELECT 
        CASE 
            WHEN LENGTH(hashed_password) > 70 THEN 'READY' 
            ELSE 'NEEDS FIX' 
        END 
     FROM core.users 
     WHERE email = 'abdu9744@gmail.com'
    ) as password_ready;

-- Expected output:
-- refresh_tokens table: ✅ EXISTS
-- password_status: ✅ PBKDF2 FORMAT (CORRECT)
-- hash_length: 84
-- is_active: true

