import { Routes } from '@angular/router';

export const routes: Routes = [
  // { path: '', redirectTo: 'login', pathMatch: 'full' },
  // { path: 'login', loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent) },
  // { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },  
  // { path: 'register', loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent) },

  {
        path: '',
        loadComponent: () =>
            import("./features/auth/login/login.component").then(
                (m) => m.LoginComponent
            ),
        // canActivate: [landingGuard],
    },

];
