import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
        path: '',
        loadComponent: () =>
            import("./accounts/accounts.component").then(
                (m) => m.AccountsComponent
            ),
    },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountingRoutingModule {

 }
