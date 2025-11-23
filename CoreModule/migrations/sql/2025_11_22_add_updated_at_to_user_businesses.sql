-- =====================================================
-- Migration: Ensure user_businesses.updated_at_utc column exists
-- Date: 2025-11-22
-- Description:
--   Some environments were provisioned before the core.user_businesses table
--   received the updated_at_utc column. The API now expects this column for
--   audit metadata, so add it when missing.
-- =====================================================

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'user_businesses'
          AND column_name = 'updated_at_utc'
    ) THEN
        ALTER TABLE core.user_businesses
            ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;

        COMMENT ON COLUMN core.user_businesses.updated_at_utc
            IS 'Last time the user-business link was updated.';
    END IF;
END $$;

COMMIT;

-- Optional: backfill existing rows so the column is not entirely null
UPDATE core.user_businesses
SET updated_at_utc = created_at_utc
WHERE updated_at_utc IS NULL;

-- Verification query
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'core'
  AND table_name = 'user_businesses'
  AND column_name = 'updated_at_utc';

