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
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
