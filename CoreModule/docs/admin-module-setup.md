# Admin Module Setup

The Admin SPA expects a handful of system objects (admin user, module definition, menu tree, etc.) to exist before it can render the full navigation. Use the scripts bundled with the repo to provision everything in one go.

## 1. Seed the Default Admin Account

```bash
psql -U <db_user> -d <db_name> \
  -f /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/migrations/sql/2025_11_23_seed_admin_ui_user.sql
```

This script makes sure `admin@encryptz.com` exists with handle `encryptzAdmin`, links it to a dedicated workspace, and assigns the Admin role. Credentials (for local/demo only):

* Email: `admin@encryptz.com`
* Handle: `encryptzAdmin`
* Password: `admin@456`

## 2. Seed the Admin Navigation Module + Menu

```bash
psql -U <db_user> -d <db_name> \
  -f /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/migrations/sql/2025_11_23_seed_admin_menu_items.sql
```

This creates the `Admin Control Center` entry inside `core.modules` plus the entire left-nav tree (dashboard, user management, security center, modules, menu builder, subscription plans, audit log). The script is idempotent—rerun it whenever you need to reset labels or routes.

## 3. Quick Verification Queries

```sql
SELECT module_id, module_name FROM core.modules WHERE LOWER(module_name) = 'admin control center';
SELECT menu_text, menu_url FROM core.menu_items WHERE module_id = 900 ORDER BY display_order;
SELECT user_handle, email FROM core.users WHERE email = 'admin@encryptz.com';
```

If the second query returns zero rows, rerun the menu seed script—without these rows the SPA falls back to a minimal hard-coded nav.

## 4. Run the API + Admin SPA

```bash
# API (from CoreModule/API/encryptzERP)
dotnet run

# Admin SPA (from CoreModule/UI/Admin/encryptz.Admin)
npm install   # first time only
npm run start
```

## 5. Notes

* The Angular sidebar will automatically fall back to a built-in navigation set if it detects an empty menu payload, so you can still explore the UI while the DB is being prepared.
* To customize navigation later, use the “Menu Builder” screen in the Admin module, which talks to `api/MenuItem`.
* Dashboards, role/permission management, menu builder, and audit log screens expect real data in their respective tables (`core.roles`, `core.permissions`, `core.menu_items`, `core.audit_logs`). Use the seed scripts or the UI itself to populate them.


