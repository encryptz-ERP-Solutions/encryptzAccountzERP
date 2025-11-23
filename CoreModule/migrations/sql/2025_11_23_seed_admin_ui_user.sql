-- =================================================================================================
-- Script: 2025_11_23_seed_admin_ui_user.sql
-- Purpose:
--   Create or update a guaranteed system admin account that can be used to
--   explore the Admin Module UI. The script is idempotent and safe to rerun.
--   It performs the following:
--     1. Ensures the "Admin" role exists.
--     2. Upserts the admin user (email: admin@encryptz.com, handle: encryptzAdmin).
--     3. Creates a dedicated admin workspace/business (if missing).
--     4. Links the user to the business and assigns the Admin role.
--
-- Credentials provisioned by this script:
--   Email       : admin@encryptz.com
--   User Handle : encryptzAdmin
--   Password    : admin@456    (PBKDF2 hash generated via GenerateHash utility)
--
-- WARNING: These credentials are for local testing only. Change the password
--          immediately if you ever promote this data to a shared/staging/prod DB.
-- =================================================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

WITH ensured_role AS (
    INSERT INTO core.roles (role_name, description, is_system_role)
    SELECT 'Admin', 'System administrator with full permissions', TRUE
    WHERE NOT EXISTS (
        SELECT 1 FROM core.roles WHERE LOWER(role_name) = 'admin'
    )
    RETURNING role_id
),
admin_role AS (
    SELECT role_id FROM ensured_role
    UNION
    SELECT role_id FROM core.roles WHERE LOWER(role_name) = 'admin' LIMIT 1
),
existing_user AS (
    SELECT user_id FROM core.users WHERE email = 'admin@encryptz.com'
),
updated_user AS (
    UPDATE core.users
    SET user_handle = 'encryptzAdmin',
        full_name = 'Encryptz System Admin',
        hashed_password = 'AQAAAAEAACcQAAAAEOcTFXuLhYKypSQ/sqIsqkM3MREzniO61rYGoFiHorZL0LUIqzaUGnmCzicXRUkKvg==',
        mobile_country_code = '+91',
        mobile_number = '9876543210',
        is_active = TRUE,
        updated_at_utc = NOW()
    WHERE email = 'admin@encryptz.com'
    RETURNING user_id
),
inserted_user AS (
    INSERT INTO core.users (
        user_id,
        user_handle,
        full_name,
        email,
        hashed_password,
        mobile_country_code,
        mobile_number,
        pan_card_number_encrypted,
        aadhar_number_encrypted,
        is_active,
        created_at_utc,
        updated_at_utc
    )
    SELECT
        '33333333-3333-3333-3333-777777777777'::uuid,
        'encryptzAdmin',
        'Encryptz System Admin',
        'admin@encryptz.com',
        'AQAAAAEAACcQAAAAEOcTFXuLhYKypSQ/sqIsqkM3MREzniO61rYGoFiHorZL0LUIqzaUGnmCzicXRUkKvg==',
        '+91',
        '9876543210',
        decode('454E435259505A41444D494E50414E', 'hex'), -- "ENCRYPZADMINPAN" placeholder
        NULL,
        TRUE,
        NOW(),
        NOW()
    WHERE NOT EXISTS (SELECT 1 FROM existing_user)
    RETURNING user_id
),
target_user AS (
    SELECT user_id FROM inserted_user
    UNION
    SELECT user_id FROM updated_user
    UNION
    SELECT user_id FROM existing_user
),
upserted_business AS (
    INSERT INTO core.businesses (
        business_id,
        business_name,
        business_code,
        is_active,
        gstin,
        tan_number,
        address_line1,
        address_line2,
        city,
        state_id,
        pin_code,
        country_id,
        created_by_user_id,
        created_at_utc,
        updated_by_user_id,
        updated_at_utc
    )
    SELECT
        '44444444-4444-4444-4444-AAAAAAAAAAAA'::uuid,
        'Encryptz Admin Control Center',
        'ADMIN-CTRL',
        TRUE,
        NULL,
        NULL,
        'Demo Tower, Level 4',
        NULL,
        'Bengaluru',
        NULL,
        '560100',
        NULL,
        tu.user_id,
        NOW(),
        tu.user_id,
        NOW()
    FROM target_user tu
    ON CONFLICT (business_id)
    DO UPDATE SET
        business_name = EXCLUDED.business_name,
        business_code = EXCLUDED.business_code,
        is_active = TRUE,
        updated_by_user_id = EXCLUDED.updated_by_user_id,
        updated_at_utc = NOW()
    RETURNING business_id
),
target_business AS (
    SELECT business_id FROM upserted_business
),
linked_business AS (
    INSERT INTO core.user_businesses (
        user_business_id,
        user_id,
        business_id,
        is_default,
        created_at_utc,
        updated_at_utc
    )
    SELECT
        gen_random_uuid(),
        tu.user_id,
        tb.business_id,
        TRUE,
        NOW(),
        NOW()
    FROM target_user tu
    CROSS JOIN target_business tb
    WHERE NOT EXISTS (
        SELECT 1
        FROM core.user_businesses ub
        WHERE ub.user_id = tu.user_id
          AND ub.business_id = tb.business_id
    )
    RETURNING user_id, business_id
)
INSERT INTO core.user_business_roles (user_id, business_id, role_id)
SELECT
    tu.user_id,
    tb.business_id,
    ar.role_id
FROM target_user tu
CROSS JOIN target_business tb
CROSS JOIN admin_role ar
WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_business_roles ubr
    WHERE ubr.user_id = tu.user_id
      AND ubr.business_id = tb.business_id
      AND ubr.role_id = ar.role_id
);

-- =================================================================================================
-- Verification (optional)
-- =================================================================================================
SELECT user_id, user_handle, email, is_active, LENGTH(hashed_password) AS hash_length
FROM core.users
WHERE email = 'admin@encryptz.com';

SELECT b.business_id, b.business_name, b.business_code, b.is_active
FROM core.businesses b
WHERE b.business_id = '44444444-4444-4444-4444-AAAAAAAAAAAA'::uuid;

SELECT ubr.user_id, ubr.business_id, r.role_name
FROM core.user_business_roles ubr
JOIN core.roles r ON r.role_id = ubr.role_id
WHERE ubr.user_id = (
    SELECT user_id FROM core.users WHERE email = 'admin@encryptz.com' LIMIT 1
);

