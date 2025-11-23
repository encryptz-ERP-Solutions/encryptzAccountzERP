-- ================================================================================================
-- Script: 2025_11_23_seed_accounts_menu_items.sql
-- Purpose: Seed modules and menu items required for the Accounts UI. The script is idempotent and can
--          be executed multiple times. Existing rows are updated with the latest labels/routes.
-- ================================================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

WITH upsert_accounts_module AS (
    INSERT INTO core.modules (module_id, module_key, module_name, is_system_module, is_active)
    VALUES (
        100,
        'ACCOUNTS',
        'Accounts',
        TRUE,
        TRUE
    )
    ON CONFLICT (module_id) DO UPDATE
        SET module_key = EXCLUDED.module_key,
            module_name = EXCLUDED.module_name,
            is_system_module = TRUE,
            is_active = TRUE
    RETURNING module_id
),
accounts_module AS (
    SELECT module_id FROM upsert_accounts_module
)
INSERT INTO core.menu_items (
    menu_item_id,
    module_id,
    parent_menu_item_id,
    menu_text,
    menu_url,
    icon_class,
    display_order,
    is_active
)
VALUES
    (1001, (SELECT module_id FROM accounts_module), NULL, 'Workspace Overview', '/accounts/dashboard', 'dashboard', 1, TRUE),
    (1002, (SELECT module_id FROM accounts_module), NULL, 'Chart of Accounts', '/accounts/chartofaccounts', 'account_tree', 2, TRUE),
    (1003, (SELECT module_id FROM accounts_module), NULL, 'Vouchers', NULL, 'receipt_long', 3, FALSE),
    (1004, (SELECT module_id FROM accounts_module), NULL, 'Ledger Reports', NULL, 'library_books', 4, FALSE),
    (1005, (SELECT module_id FROM accounts_module), NULL, 'Trial Balance', NULL, 'analytics', 5, FALSE)
ON CONFLICT (menu_item_id) DO UPDATE
    SET module_id = EXCLUDED.module_id,
        parent_menu_item_id = EXCLUDED.parent_menu_item_id,
        menu_text = EXCLUDED.menu_text,
        menu_url = EXCLUDED.menu_url,
        icon_class = EXCLUDED.icon_class,
        display_order = EXCLUDED.display_order,
        is_active = EXCLUDED.is_active;

-- Verification
SELECT menu_text, menu_url, display_order, is_active
FROM core.menu_items
WHERE module_id = (SELECT module_id FROM core.modules WHERE module_key = 'ACCOUNTS')
ORDER BY display_order, menu_text;

