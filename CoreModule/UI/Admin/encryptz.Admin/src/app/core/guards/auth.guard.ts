import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';
import { Observable } from 'rxjs';

/**
 * Auth Guard to protect routes from unauthorized access
 * 
 * Usage in routes:
 * {
 *   path: 'dashboard',
 *   component: DashboardComponent,
 *   canActivate: [authGuard]
 * }
 */
export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (isLoggedIn && authService.getAccessToken()) {
        // User is authenticated
        return true;
      } else {
        // User is not authenticated, redirect to login
        // Preserve the attempted URL for redirecting after login
        router.navigate(['/'], {
          queryParams: { returnUrl: state.url }
        });
        return false;
      }
    })
  );
};

/**
 * Inverse auth guard - redirects authenticated users away from auth pages
 * Useful for login/register pages
 * 
 * Usage in routes:
 * {
 *   path: 'login',
 *   component: LoginComponent,
 *   canActivate: [guestGuard]
 * }
 */
export const guestGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (isLoggedIn && authService.getAccessToken()) {
        // User is already authenticated, redirect to dashboard
        router.navigate(['/dashboard']);
        return false;
      } else {
        // User is not authenticated, allow access to login page
        return true;
      }
    })
  );
};
