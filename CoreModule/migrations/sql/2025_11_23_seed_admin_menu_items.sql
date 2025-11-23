-- ================================================================================================
-- Script: 2025_11_23_seed_admin_menu_items.sql
-- Purpose: Seed modules and menu items required for the Admin UI. The script is idempotent and can
--          be executed multiple times. Existing rows are updated with the latest labels/routes.
-- ================================================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

WITH upsert_admin_module AS (
    INSERT INTO core.modules (module_id, module_key, module_name, is_system_module, is_active)
    VALUES (
        900,
        'ADMIN_CONTROL_CENTER',
        'Admin Control Center',
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
admin_module AS (
    SELECT module_id FROM upsert_admin_module
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
    (9001, (SELECT module_id FROM admin_module), NULL, 'Dashboard', '/admin/dashboard', 'dashboard', 1, TRUE),
    (9002, (SELECT module_id FROM admin_module), NULL, 'User Management', '/admin/user-management', 'group', 2, TRUE),
    (9003, (SELECT module_id FROM admin_module), NULL, 'Security Center', NULL, 'admin_panel_settings', 3, TRUE),
    (9004, (SELECT module_id FROM admin_module), 9003, 'Roles', '/admin/roles', 'badge', 1, TRUE),
    (9005, (SELECT module_id FROM admin_module), 9003, 'Permissions', '/admin/permissions', 'checklist', 2, TRUE),
    (9006, (SELECT module_id FROM admin_module), NULL, 'System Setup', NULL, 'settings', 4, TRUE),
    (9007, (SELECT module_id FROM admin_module), 9006, 'Modules', '/admin/modules', 'widgets', 1, TRUE),
    (9008, (SELECT module_id FROM admin_module), 9006, 'Menu Builder', '/admin/menu-builder', 'menu', 2, TRUE),
    (9009, (SELECT module_id FROM admin_module), NULL, 'Subscription Plans', '/admin/subscription-plans', 'price_change', 5, TRUE),
    (9010, (SELECT module_id FROM admin_module), NULL, 'Audit & Activity', '/admin/audit-log', 'history', 6, TRUE)
ON CONFLICT (menu_item_id) DO UPDATE
    SET module_id = EXCLUDED.module_id,
        parent_menu_item_id = EXCLUDED.parent_menu_item_id,
        menu_text = EXCLUDED.menu_text,
        menu_url = EXCLUDED.menu_url,
        icon_class = EXCLUDED.icon_class,
        display_order = EXCLUDED.display_order,
        is_active = TRUE;

-- Verification
SELECT menu_text, menu_url, display_order
FROM core.menu_items
WHERE module_id = (SELECT module_id FROM core.modules WHERE module_key = 'ADMIN_CONTROL_CENTER')
ORDER BY display_order, menu_text;

