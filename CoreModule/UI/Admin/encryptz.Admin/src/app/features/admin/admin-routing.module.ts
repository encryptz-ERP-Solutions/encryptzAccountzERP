import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import("./admin-layout/admin-layout.component").then(
        (m) => m.AdminLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/admin-dashboard/admin-dashboard.component').then(
            (m) => m.AdminDashboardComponent
          )
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./components/admin-dashboard/admin-dashboard.component').then(
            (m) => m.AdminDashboardComponent
          ),
        data: { title: 'Dashboard' }
      },
      {
        path: 'user-management',
        loadComponent: () =>
          import('./components/user-management/user-management.component').then(
            (m) => m.UserManagementComponent
          ),
        data: { title: 'user-management' }
      },
      {
        path: 'roles',
        loadComponent: () =>
          import('./components/roles/roles.component').then(
            (m) => m.RolesComponent
          ),
        data: { title: 'Roles' }
      },
      {
        path: 'permissions',
        loadComponent: () =>
          import('./components/permissions/permissions.component').then(
            (m) => m.PermissionsComponent
          ),
        data: { title: 'Permissions' }
      },
      {
        path: 'role-permissions',
        loadComponent: () =>
          import('./components/role-permissions/role-permissions.component').then(
            (m) => m.RolePermissionsComponent
          ),
        data: { title: 'Role Permissions' }
      },
      {
        path: 'modules',
        loadComponent: () =>
          import('./components/modules/modules.component').then(
            (m) => m.ModulesComponent
          ),
        data: { title: 'Modules' }
      },
      {
        path: 'menu-builder',
        loadComponent: () =>
          import('./components/menu-builder/menu-builder.component').then(
            (m) => m.MenuBuilderComponent
          ),
        data: { title: 'Menu Builder' }
      },
      {
        path: 'subscription-plans',
        loadComponent: () =>
          import('./components/subscription-plans/subscription-plans.component').then(
            (m) => m.SubscriptionPlansComponent
          ),
        data: { title: 'Subscription Plans' }
      },
      {
        path: 'audit-log',
        loadComponent: () =>
          import('./components/audit-log/audit-log.component').then(
            (m) => m.AuditLogComponent
          ),
        data: { title: 'Audit Log' }
      },
      {
        path: 'business-management',
        loadComponent: () =>
          import('./components/business-management/business-management.component').then(
            (m) => m.BusinessManagementComponent
          ),
        data: { title: 'Business Management' }
      },
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
