import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';
import { Observable } from 'rxjs';

/**
 * Role Guard to protect routes based on user roles
 * 
 * Usage in routes:
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [roleGuard],
 *   data: { roles: ['Admin', 'SuperAdmin'] }
 * }
 * 
 * Or with single role:
 * {
 *   path: 'manager',
 *   component: ManagerComponent,
 *   canActivate: [roleGuard],
 *   data: { roles: ['Manager'] }
 * }
 */
export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Get required roles from route data
  const requiredRoles = route.data['roles'] as string[];

  if (!requiredRoles || requiredRoles.length === 0) {
    console.warn('roleGuard: No roles specified in route data');
    return authService.isLoggedIn$.pipe(
      take(1),
      map(isLoggedIn => isLoggedIn)
    );
  }

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (!isLoggedIn || !authService.getAccessToken()) {
        // User is not authenticated, redirect to login
        router.navigate(['/'], {
          queryParams: { returnUrl: state.url }
        });
        return false;
      }

      // Check if user has any of the required roles
      const hasRole = authService.hasAnyRole(requiredRoles);
      
      if (hasRole) {
        return true;
      } else {
        // User doesn't have required role, redirect to unauthorized page or dashboard
        console.warn('User does not have required role:', requiredRoles);
        router.navigate(['/dashboard'], {
          queryParams: { error: 'unauthorized' }
        });
        return false;
      }
    })
  );
};

/**
 * Permission Guard to protect routes based on user permissions
 * 
 * Usage in routes:
 * {
 *   path: 'users/edit',
 *   component: UserEditComponent,
 *   canActivate: [permissionGuard],
 *   data: { permissions: ['user.edit', 'user.manage'] }
 * }
 */
export const permissionGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Get required permissions from route data
  const requiredPermissions = route.data['permissions'] as string[];

  if (!requiredPermissions || requiredPermissions.length === 0) {
    console.warn('permissionGuard: No permissions specified in route data');
    return authService.isLoggedIn$.pipe(
      take(1),
      map(isLoggedIn => isLoggedIn)
    );
  }

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (!isLoggedIn || !authService.getAccessToken()) {
        // User is not authenticated, redirect to login
        router.navigate(['/'], {
          queryParams: { returnUrl: state.url }
        });
        return false;
      }

      // Check if user has any of the required permissions
      const hasPermission = authService.hasAnyPermission(requiredPermissions);
      
      if (hasPermission) {
        return true;
      } else {
        // User doesn't have required permission
        console.warn('User does not have required permission:', requiredPermissions);
        router.navigate(['/dashboard'], {
          queryParams: { error: 'forbidden' }
        });
        return false;
      }
    })
  );
};

/**
 * Combined guard that checks both authentication and roles
 * This is a convenience function that combines authGuard and roleGuard
 * 
 * Usage:
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [authAndRoleGuard],
 *   data: { roles: ['Admin'] }
 * }
 */
export const authAndRoleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> => {
  // This effectively combines both guards
  return roleGuard(route, state);
};
