import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import("./accounts-layout/accounts-layout.component").then(
        (m) => m.AccountsLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/accounts-dashboard/accounts-dashboard.component').then(
            (m) => m.AccountsDashboardComponent
          )
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./components/accounts-dashboard/accounts-dashboard.component').then(
            (m) => m.AccountsDashboardComponent
          )
      },
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountsRoutingModule { }
