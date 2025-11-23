-- Date: 2025-11-23
-- Purpose:
--   Align the core.user_businesses primary key with the application model.
--   Older databases created this table with a BIGINT/bigserial primary key,
--   but the API now expects UUID values for user_business_id.
--   This script converts the column to UUID without losing existing rows.
--
-- Safety:
--   - Wraps everything in a transaction.
--   - Creates fresh UUIDs for every row (there are no external references to the old numeric ID).
--   - Rebuilds the primary key and drops the legacy sequence.

BEGIN;

-- Ensure we can call gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Add the new UUID column alongside the legacy bigint column
ALTER TABLE core.user_businesses
    ADD COLUMN IF NOT EXISTS user_business_id_uuid UUID DEFAULT gen_random_uuid();

UPDATE core.user_businesses
SET user_business_id_uuid = gen_random_uuid()
WHERE user_business_id_uuid IS NULL;

ALTER TABLE core.user_businesses
    ALTER COLUMN user_business_id_uuid SET NOT NULL;

-- Drop existing PK so we can swap the columns
ALTER TABLE core.user_businesses
    DROP CONSTRAINT IF EXISTS core_user_businesses_pkey,
    DROP CONSTRAINT IF EXISTS user_businesses_pkey;

-- Remove the old bigint column and rename the UUID column into its place
ALTER TABLE core.user_businesses
    DROP COLUMN user_business_id;

ALTER TABLE core.user_businesses
    RENAME COLUMN user_business_id_uuid TO user_business_id;

-- Recreate the primary key and default
ALTER TABLE core.user_businesses
    ADD PRIMARY KEY (user_business_id);

ALTER TABLE core.user_businesses
    ALTER COLUMN user_business_id SET DEFAULT gen_random_uuid();

-- Clean up the obsolete sequence if it exists
DROP SEQUENCE IF EXISTS core.user_businesses_user_business_id_seq;

COMMIT;

