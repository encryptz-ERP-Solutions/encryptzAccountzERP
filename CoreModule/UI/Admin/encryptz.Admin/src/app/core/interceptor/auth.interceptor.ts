import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn, HttpEvent, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, switchMap, filter, take, throwError, of } from 'rxjs';
import { CommonService } from '../../shared/services/common.service';
import { AuthService } from '../services/auth.service';

/**
 * Auth Interceptor that:
 * 1. Attaches Authorization header with access token
 * 2. Handles 401 errors by refreshing token and retrying request
 * 3. Manages concurrent refresh requests
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const commonService = inject(CommonService);
  const router = inject(Router);

  // Don't add auth header to auth endpoints (login, refresh) - but logout needs auth token
  const isPublicAuthEndpoint = req.url.includes('/auth/login') || 
                               req.url.includes('/auth/refresh');
  const isLogoutEndpoint = req.url.includes('/auth/logout');

  const shouldShowLoader = !req.headers.has('no-loader');
  if (shouldShowLoader) {
    commonService.loaderState(true);
  }

  // Clone request and add auth header
  let authReq = req;
  if (isLogoutEndpoint) {
    // Logout endpoint requires authentication, so add token
    const token = authService.getAccessToken();
    if (token) {
      authReq = addTokenToRequest(req, token);
    } else {
      // No token available, just clone request
      authReq = req.clone({
        headers: req.headers.delete('no-loader')
      });
    }
  } else if (!isPublicAuthEndpoint) {
    // For all other endpoints, add token if available
    const token = authService.getAccessToken();
    if (token) {
      authReq = addTokenToRequest(req, token);
    } else {
      // Remove no-loader header if present
      authReq = req.clone({
        headers: req.headers.delete('no-loader')
      });
    }
  } else {
    // For public auth endpoints (login, refresh), just remove the no-loader header
    authReq = req.clone({
      headers: req.headers.delete('no-loader')
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      // Don't retry logout on 401 - just let it fail gracefully
      if (error.status === 401 && !isPublicAuthEndpoint && !isLogoutEndpoint) {
        return handle401Error(authReq, next, authService, router);
      }
      
      // For logout 401 errors, clear session and redirect (don't throw error)
      if (error.status === 401 && isLogoutEndpoint) {
        authService.clearSession();
        router.navigate(['/']);
        // Return a successful response to prevent error propagation
        const successResponse: HttpEvent<unknown> = new HttpResponse({ 
          status: 200, 
          statusText: 'OK',
          body: { message: 'Logged out successfully' } 
        });
        return of(successResponse);
      }
      
      // For other errors, just pass them through
      return throwError(() => error);
    }),
    finalize(() => {
      if (shouldShowLoader) {
        commonService.loaderState(false);
      }
    })
  );
};

/**
 * Add authorization token to request
 */
function addTokenToRequest(req: HttpRequest<any>, token: string): HttpRequest<any> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    },
    headers: req.headers.delete('no-loader')
  });
}

/**
 * Handle 401 errors by attempting to refresh token
 * Uses a queuing mechanism to prevent multiple simultaneous refresh calls
 */
function handle401Error(
  request: HttpRequest<any>, 
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
) {
  // If not already refreshing, initiate refresh
  if (!authService.isRefreshingToken()) {
    authService.setRefreshing(true);
    authService.setRefreshTokenSubject(null);

    return authService.refresh().pipe(
      switchMap((response) => {
        const token = response?.accessToken;
        if (token) {
          // Refresh successful, update subject and retry original request
          authService.setRefreshing(false);
          authService.setRefreshTokenSubject(token);
          
          // Retry original request with new token
          const retryReq = addTokenToRequest(request, token);
          return next(retryReq);
        } else {
          // Refresh failed, redirect to login
          authService.setRefreshing(false);
          router.navigate(['/']);
          return throwError(() => new Error('Token refresh failed'));
        }
      }),
      catchError((error) => {
        // Refresh failed, clear session and redirect to login
        authService.setRefreshing(false);
        authService.setRefreshTokenSubject(null);
        router.navigate(['/']);
        return throwError(() => error);
      })
    );
  } else {
    // If already refreshing, wait for the refresh to complete
    return authService.getRefreshTokenObservable().pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        // Retry original request with new token
        const retryReq = addTokenToRequest(request, token!);
        return next(retryReq);
      })
    );
  }
}
