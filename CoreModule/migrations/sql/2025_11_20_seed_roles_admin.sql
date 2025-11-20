-- =====================================================
-- ERP System - Roles and Permissions Seed Script
-- Date: 2025-11-20
-- Description: Idempotent script to seed system roles and permissions
-- =====================================================

-- =====================================================
-- SECTION 1: SEED PERMISSIONS
-- =====================================================

-- Insert permissions if they don't already exist
-- Using INSERT ON CONFLICT to make this idempotent

-- User Management Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000001'::uuid, 'user.create', 'Create User', 'Create new users in the system', 'User Management', true),
    ('11111111-1111-1111-1111-000000000002'::uuid, 'user.read', 'View User', 'View user details', 'User Management', true),
    ('11111111-1111-1111-1111-000000000003'::uuid, 'user.update', 'Update User', 'Update user information', 'User Management', true),
    ('11111111-1111-1111-1111-000000000004'::uuid, 'user.delete', 'Delete User', 'Delete users from the system', 'User Management', true),
    ('11111111-1111-1111-1111-000000000005'::uuid, 'user.list', 'List Users', 'View list of all users', 'User Management', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Business Management Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000011'::uuid, 'business.create', 'Create Business', 'Create new business entities', 'Business Management', true),
    ('11111111-1111-1111-1111-000000000012'::uuid, 'business.read', 'View Business', 'View business details', 'Business Management', true),
    ('11111111-1111-1111-1111-000000000013'::uuid, 'business.update', 'Update Business', 'Update business information', 'Business Management', true),
    ('11111111-1111-1111-1111-000000000014'::uuid, 'business.delete', 'Delete Business', 'Delete business entities', 'Business Management', true),
    ('11111111-1111-1111-1111-000000000015'::uuid, 'business.list', 'List Businesses', 'View list of all businesses', 'Business Management', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Accounting Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000021'::uuid, 'voucher.create', 'Create Voucher', 'Create new accounting vouchers', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000022'::uuid, 'voucher.read', 'View Voucher', 'View voucher details', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000023'::uuid, 'voucher.update', 'Update Voucher', 'Update voucher information', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000024'::uuid, 'voucher.delete', 'Delete Voucher', 'Delete vouchers', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000025'::uuid, 'voucher.post', 'Post Voucher', 'Post vouchers to ledger', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000026'::uuid, 'voucher.approve', 'Approve Voucher', 'Approve vouchers for posting', 'Accounting', true),
    ('11111111-1111-1111-1111-000000000027'::uuid, 'voucher.void', 'Void Voucher', 'Void posted vouchers', 'Accounting', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Chart of Accounts Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000031'::uuid, 'coa.create', 'Create Account', 'Create chart of accounts entries', 'Chart of Accounts', true),
    ('11111111-1111-1111-1111-000000000032'::uuid, 'coa.read', 'View Account', 'View account details', 'Chart of Accounts', true),
    ('11111111-1111-1111-1111-000000000033'::uuid, 'coa.update', 'Update Account', 'Update account information', 'Chart of Accounts', true),
    ('11111111-1111-1111-1111-000000000034'::uuid, 'coa.delete', 'Delete Account', 'Delete accounts', 'Chart of Accounts', true),
    ('11111111-1111-1111-1111-000000000035'::uuid, 'coa.list', 'List Accounts', 'View chart of accounts', 'Chart of Accounts', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Reports Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000041'::uuid, 'report.trial_balance', 'View Trial Balance', 'Access trial balance report', 'Reports', true),
    ('11111111-1111-1111-1111-000000000042'::uuid, 'report.profit_loss', 'View P&L', 'Access profit and loss report', 'Reports', true),
    ('11111111-1111-1111-1111-000000000043'::uuid, 'report.balance_sheet', 'View Balance Sheet', 'Access balance sheet report', 'Reports', true),
    ('11111111-1111-1111-1111-000000000044'::uuid, 'report.ledger', 'View Ledger', 'Access account ledger reports', 'Reports', true),
    ('11111111-1111-1111-1111-000000000045'::uuid, 'report.daybook', 'View Day Book', 'Access day book report', 'Reports', true),
    ('11111111-1111-1111-1111-000000000046'::uuid, 'report.gst', 'View GST Reports', 'Access GST compliance reports', 'Reports', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Inventory Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000051'::uuid, 'inventory.create', 'Create Item', 'Create inventory items', 'Inventory', true),
    ('11111111-1111-1111-1111-000000000052'::uuid, 'inventory.read', 'View Item', 'View inventory item details', 'Inventory', true),
    ('11111111-1111-1111-1111-000000000053'::uuid, 'inventory.update', 'Update Item', 'Update inventory items', 'Inventory', true),
    ('11111111-1111-1111-1111-000000000054'::uuid, 'inventory.delete', 'Delete Item', 'Delete inventory items', 'Inventory', true),
    ('11111111-1111-1111-1111-000000000055'::uuid, 'inventory.adjust', 'Adjust Stock', 'Make stock adjustments', 'Inventory', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Role Management Permissions
INSERT INTO permissions (permission_id, permission_code, permission_name, permission_description, module, is_active)
VALUES 
    ('11111111-1111-1111-1111-000000000061'::uuid, 'role.create', 'Create Role', 'Create new roles', 'Role Management', true),
    ('11111111-1111-1111-1111-000000000062'::uuid, 'role.read', 'View Role', 'View role details', 'Role Management', true),
    ('11111111-1111-1111-1111-000000000063'::uuid, 'role.update', 'Update Role', 'Update role information', 'Role Management', true),
    ('11111111-1111-1111-1111-000000000064'::uuid, 'role.delete', 'Delete Role', 'Delete roles', 'Role Management', true),
    ('11111111-1111-1111-1111-000000000065'::uuid, 'role.assign', 'Assign Role', 'Assign roles to users', 'Role Management', true)
ON CONFLICT (permission_code) DO UPDATE 
SET 
    permission_name = EXCLUDED.permission_name,
    permission_description = EXCLUDED.permission_description,
    module = EXCLUDED.module,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- =====================================================
-- SECTION 2: SEED SYSTEM ROLES
-- =====================================================

-- Insert system roles if they don't already exist
-- These are global system roles (business_id = NULL)

INSERT INTO roles (role_id, role_name, role_description, is_system_role, business_id, is_active, permissions)
VALUES 
    ('22222222-2222-2222-2222-000000000001'::uuid, 'Admin', 'Full system administrator with all permissions', true, NULL, true, '[]'::jsonb),
    ('22222222-2222-2222-2222-000000000002'::uuid, 'Accountant', 'Accountant role with full accounting permissions', true, NULL, true, '[]'::jsonb),
    ('22222222-2222-2222-2222-000000000003'::uuid, 'Manager', 'Manager role with business and reporting permissions', true, NULL, true, '[]'::jsonb),
    ('22222222-2222-2222-2222-000000000004'::uuid, 'User', 'Standard user with basic permissions', true, NULL, true, '[]'::jsonb),
    ('22222222-2222-2222-2222-000000000005'::uuid, 'Viewer', 'Read-only viewer with reporting permissions', true, NULL, true, '[]'::jsonb)
ON CONFLICT (role_id) 
DO UPDATE SET
    role_name = EXCLUDED.role_name,
    role_description = EXCLUDED.role_description,
    is_system_role = EXCLUDED.is_system_role,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- =====================================================
-- SECTION 3: MAP PERMISSIONS TO ROLES
-- =====================================================

-- Admin Role - All permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    '22222222-2222-2222-2222-000000000001'::uuid,
    permission_id
FROM permissions
WHERE is_active = true
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Accountant Role - All accounting, COA, reports, and inventory permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    '22222222-2222-2222-2222-000000000002'::uuid,
    permission_id
FROM permissions
WHERE is_active = true
  AND (module IN ('Accounting', 'Chart of Accounts', 'Reports', 'Inventory')
       OR permission_code IN ('user.read', 'user.list', 'business.read', 'business.list'))
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Manager Role - Business, users (not delete), reports, and basic voucher permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    '22222222-2222-2222-2222-000000000003'::uuid,
    permission_id
FROM permissions
WHERE is_active = true
  AND (module IN ('Business Management', 'Reports')
       OR permission_code IN (
           'user.create', 'user.read', 'user.update', 'user.list',
           'voucher.read', 'voucher.create', 'voucher.approve',
           'coa.read', 'coa.list',
           'inventory.read', 'inventory.create', 'inventory.update'
       ))
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- User Role - Basic read/create permissions for vouchers and inventory
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    '22222222-2222-2222-2222-000000000004'::uuid,
    permission_id
FROM permissions
WHERE is_active = true
  AND permission_code IN (
      'voucher.create', 'voucher.read',
      'coa.read', 'coa.list',
      'inventory.create', 'inventory.read', 'inventory.update',
      'business.read', 'business.list',
      'report.ledger', 'report.daybook'
  )
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Viewer Role - Read-only access to reports and data
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    '22222222-2222-2222-2222-000000000005'::uuid,
    permission_id
FROM permissions
WHERE is_active = true
  AND (module = 'Reports'
       OR permission_code IN (
           'user.read', 'user.list',
           'business.read', 'business.list',
           'voucher.read',
           'coa.read', 'coa.list',
           'inventory.read'
       ))
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- =====================================================
-- VERIFICATION QUERIES (OPTIONAL - COMMENT OUT IF NOT NEEDED)
-- =====================================================

-- Count permissions created
DO $$
DECLARE
    permission_count INTEGER;
    role_count INTEGER;
    role_permission_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO permission_count FROM permissions;
    SELECT COUNT(*) INTO role_count FROM roles WHERE is_system_role = true;
    SELECT COUNT(*) INTO role_permission_count FROM role_permissions 
    WHERE role_id IN (SELECT role_id FROM roles WHERE is_system_role = true);
    
    RAISE NOTICE 'Seed script completed successfully:';
    RAISE NOTICE '  - Total Permissions: %', permission_count;
    RAISE NOTICE '  - System Roles: %', role_count;
    RAISE NOTICE '  - Role-Permission Mappings: %', role_permission_count;
END $$;

-- =====================================================
-- NOTES
-- =====================================================
-- 
-- 1. This script is idempotent - it can be run multiple times safely
-- 2. UUIDs are hardcoded for system roles and permissions for consistency
-- 3. System roles have business_id = NULL (global roles)
-- 4. Business-specific roles can be added separately with a business_id
-- 5. To add custom permissions, follow the same pattern with unique UUIDs
-- 6. Permission codes follow the pattern: module.action (e.g., voucher.create)
-- =====================================================
