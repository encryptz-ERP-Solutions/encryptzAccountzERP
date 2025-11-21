# Angular Routes Configuration with Auth Guards

This document shows how to configure your application routes with the new authentication guards.

## Complete Routes Example

Update your `app.routes.ts` file:

```typescript
import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { roleGuard, permissionGuard } from './core/guards/role.guard';

export const routes: Routes = [
    // ============= Public Routes (No Authentication Required) =============
    
    {
        path: '',
        loadComponent: () =>
            import("./features/auth/login/login.component").then(
                (m) => m.LoginComponent
            ),
        canActivate: [guestGuard], // Redirect to dashboard if already logged in
    },
    {
        path: 'register',
        loadComponent: () =>
            import("./features/auth/register/register.component").then(
                (m) => m.RegisterComponent
            ),
        canActivate: [guestGuard],
    },

    // ============= Protected Routes (Authentication Required) =============
    
    {
        path: 'dashboard',
        loadComponent: () =>
            import("./features/layout/layout.component").then(
                (m) => m.LayoutComponent
            ),
        canActivate: [authGuard], // Require authentication
        children: [
            {
                path: '',
                loadComponent: () =>
                    import('./features/dashboard/dashboard.component').then(
                        (m) => m.DashboardComponent
                    )
            },
        ]
    },

    // ============= Role-Based Protected Routes =============
    
    {
        path: 'accounts',
        loadChildren: () =>
            import('./features/accounts/accounts.module').then(
                (m) => m.AccountsModule
            ),
        canActivate: [authGuard], // Require authentication
    },
    
    {
        path: 'admin',
        loadChildren: () =>
            import("./features/admin/admin.module").then(
                (m) => m.AdminModule
            ),
        canActivate: [roleGuard], // Require specific role
        data: { roles: ['Admin', 'SuperAdmin'] } // Only Admin or SuperAdmin can access
    },

    // ============= Advanced Role and Permission Guards =============
    
    {
        path: 'manager',
        loadComponent: () =>
            import('./features/manager/manager.component').then(
                (m) => m.ManagerComponent
            ),
        canActivate: [roleGuard],
        data: { roles: ['Manager', 'Admin'] }
    },
    
    {
        path: 'users',
        loadComponent: () =>
            import('./features/users/user-list.component').then(
                (m) => m.UserListComponent
            ),
        canActivate: [permissionGuard],
        data: { permissions: ['user.view', 'user.list'] }
    },
    
    {
        path: 'users/edit/:id',
        loadComponent: () =>
            import('./features/users/user-edit.component').then(
                (m) => m.UserEditComponent
            ),
        canActivate: [permissionGuard],
        data: { permissions: ['user.edit', 'user.manage'] }
    },

    // ============= Wildcard Route =============
    
    {
        path: '**',
        redirectTo: '',
        pathMatch: 'full'
    }
];
```

## Guard Usage Patterns

### 1. Basic Authentication Guard

Protects routes from unauthenticated access:

```typescript
{
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
}
```

### 2. Guest Guard (Inverse Auth)

Prevents authenticated users from accessing login/register pages:

```typescript
{
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard]
}
```

### 3. Role-Based Guard

Restricts access to users with specific roles:

```typescript
{
    path: 'admin',
    component: AdminComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin'] } // Single role
}

// Or multiple roles (user must have ANY of these)
{
    path: 'management',
    component: ManagementComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'Manager', 'SuperAdmin'] }
}
```

### 4. Permission-Based Guard

Restricts access based on granular permissions:

```typescript
{
    path: 'users/delete',
    component: DeleteUserComponent,
    canActivate: [permissionGuard],
    data: { permissions: ['user.delete', 'user.manage'] }
}
```

### 5. Multiple Guards

You can combine multiple guards (they run in order):

```typescript
{
    path: 'admin/users',
    component: AdminUsersComponent,
    canActivate: [authGuard, roleGuard], // First check auth, then role
    data: { roles: ['Admin'] }
}
```

### 6. Lazy-Loaded Modules with Guards

```typescript
{
    path: 'accounts',
    loadChildren: () => import('./features/accounts/accounts.module').then(m => m.AccountsModule),
    canActivate: [authGuard] // Guard applies to all routes in module
}
```

## Child Routes with Guards

Guards can be applied at different levels:

```typescript
{
    path: 'dashboard',
    component: LayoutComponent,
    canActivate: [authGuard], // Parent guard
    children: [
        {
            path: 'overview',
            component: OverviewComponent
            // Inherits parent guard
        },
        {
            path: 'admin',
            component: AdminDashboardComponent,
            canActivate: [roleGuard], // Additional child guard
            data: { roles: ['Admin'] }
        }
    ]
}
```

## Error Handling in Routes

Create an unauthorized component for better UX:

```typescript
{
    path: 'unauthorized',
    loadComponent: () => import('./features/errors/unauthorized.component').then(m => m.UnauthorizedComponent)
}
```

Then in your guards, redirect there:

```typescript
// In role.guard.ts
if (!hasRole) {
    router.navigate(['/unauthorized']);
    return false;
}
```

## Navigation with Return URLs

The auth guard automatically preserves the return URL:

```typescript
// User tries to access /dashboard without being logged in
// Guard redirects to: /?returnUrl=/dashboard

// After login, redirect to returnUrl:
constructor(private route: ActivatedRoute, private router: Router) {}

onLoginSuccess() {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    this.router.navigate([returnUrl]);
}
```

## Testing Routes

Test your routes configuration:

```typescript
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { routes } from './app.routes';

describe('App Routes', () => {
    let router: Router;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule.withRoutes(routes)]
        });
        router = TestBed.inject(Router);
    });

    it('should navigate to login for root path', () => {
        const url = router.serializeUrl(router.createUrlTree(['/']));
        expect(url).toBe('/');
    });

    it('should have dashboard route', () => {
        const route = routes.find(r => r.path === 'dashboard');
        expect(route).toBeDefined();
    });
});
```

## Best Practices

1. **Apply guards at the highest level possible** - Reduces redundancy
2. **Use lazy loading for better performance** - Load routes on demand
3. **Keep route data consistent** - Use same property names for roles/permissions
4. **Create error pages** - Provide good UX for unauthorized access
5. **Test all routes** - Ensure guards work as expected
6. **Document route permissions** - Make it clear which roles can access what

## Common Patterns

### Admin Section

```typescript
{
    path: 'admin',
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SuperAdmin'] },
    children: [
        { path: 'users', component: UserManagementComponent },
        { path: 'settings', component: SettingsComponent },
        { path: 'reports', component: ReportsComponent }
    ]
}
```

### User Profile (Self or Admin)

```typescript
{
    path: 'profile/:id',
    component: ProfileComponent,
    canActivate: [authGuard]
    // Component logic handles "can user edit this profile?"
}
```

### Feature Flags

You can combine guards with feature flags:

```typescript
{
    path: 'beta-feature',
    component: BetaFeatureComponent,
    canActivate: [authGuard, featureGuard],
    data: { feature: 'beta_access' }
}
```
