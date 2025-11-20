-- Migration: Create refresh_tokens table for JWT authentication
-- Author: System
-- Date: 2025-11-20
-- Description: Creates the refresh_tokens table for storing hashed refresh tokens with rotation support

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
    created_by_ip VARCHAR(45) NULL,  -- Supports IPv6
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

-- Add comments for documentation
COMMENT ON TABLE core.refresh_tokens IS 'Stores hashed refresh tokens for JWT authentication with rotation support';
COMMENT ON COLUMN core.refresh_tokens.refresh_token_id IS 'Unique identifier for the refresh token';
COMMENT ON COLUMN core.refresh_tokens.user_id IS 'User who owns this refresh token';
COMMENT ON COLUMN core.refresh_tokens.token_hash IS 'SHA256 hash of the refresh token (stored securely, not plaintext)';
COMMENT ON COLUMN core.refresh_tokens.expires_at_utc IS 'When this refresh token expires';
COMMENT ON COLUMN core.refresh_tokens.created_at_utc IS 'When this refresh token was created';
COMMENT ON COLUMN core.refresh_tokens.is_revoked IS 'Whether this token has been revoked';
COMMENT ON COLUMN core.refresh_tokens.revoked_at_utc IS 'When this token was revoked';
COMMENT ON COLUMN core.refresh_tokens.replaced_by_token_id IS 'ID of the token that replaced this one (token rotation)';
COMMENT ON COLUMN core.refresh_tokens.created_by_ip IS 'IP address that created this token';
COMMENT ON COLUMN core.refresh_tokens.revoked_by_ip IS 'IP address that revoked this token';

-- Create a function to automatically clean up expired tokens (optional, can be run as a scheduled job)
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

COMMENT ON FUNCTION core.cleanup_expired_refresh_tokens() IS 'Deletes expired refresh tokens and revoked tokens older than 30 days';

-- Grant necessary permissions (adjust as needed for your security model)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON core.refresh_tokens TO your_app_user;
-- GRANT USAGE ON SCHEMA core TO your_app_user;

-- Migration completed successfully
-- Run the following to verify:
-- SELECT table_name FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'refresh_tokens';
-- SELECT routine_name FROM information_schema.routines WHERE routine_schema = 'core' AND routine_name = 'cleanup_expired_refresh_tokens';

