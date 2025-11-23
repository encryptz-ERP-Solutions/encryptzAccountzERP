import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { roleGuard, permissionGuard } from './role.guard';
import { AuthService } from '../services/auth.service';
import { of } from 'rxjs';

describe('RoleGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let route: ActivatedRouteSnapshot;
  let state: RouterStateSnapshot;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', [
      'getAccessToken',
      'hasAnyRole',
      'hasAnyPermission'
    ], {
      isLoggedIn$: of(false)
    });
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    
    route = { data: {} } as ActivatedRouteSnapshot;
    state = { url: '/admin' } as RouterStateSnapshot;
  });

  describe('roleGuard', () => {
    it('should allow access when user has required role', (done) => {
      route.data = { roles: ['Admin'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyRole.and.returnValue(true);

      TestBed.runInInjectionContext(() => {
        const result = roleGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
          done();
        });
      });
    });

    it('should deny access when user does not have required role', (done) => {
      route.data = { roles: ['Admin'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyRole.and.returnValue(false);

      TestBed.runInInjectionContext(() => {
        const result = roleGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(false);
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard'], {
            queryParams: { error: 'unauthorized' }
          });
          done();
        });
      });
    });

    it('should redirect to login when user is not authenticated', (done) => {
      route.data = { roles: ['Admin'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(false) });
      authServiceSpy.getAccessToken.and.returnValue(null);

      TestBed.runInInjectionContext(() => {
        const result = roleGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(false);
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/'], {
            queryParams: { returnUrl: '/admin' }
          });
          done();
        });
      });
    });

    it('should handle multiple required roles', (done) => {
      route.data = { roles: ['Admin', 'Manager'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyRole.and.returnValue(true);

      TestBed.runInInjectionContext(() => {
        const result = roleGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin', 'Manager']);
          done();
        });
      });
    });
  });

  describe('permissionGuard', () => {
    it('should allow access when user has required permission', (done) => {
      route.data = { permissions: ['user.edit'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyPermission.and.returnValue(true);

      TestBed.runInInjectionContext(() => {
        const result = permissionGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(authServiceSpy.hasAnyPermission).toHaveBeenCalledWith(['user.edit']);
          done();
        });
      });
    });

    it('should deny access when user does not have required permission', (done) => {
      route.data = { permissions: ['user.delete'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyPermission.and.returnValue(false);

      TestBed.runInInjectionContext(() => {
        const result = permissionGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(false);
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard'], {
            queryParams: { error: 'forbidden' }
          });
          done();
        });
      });
    });

    it('should handle multiple required permissions', (done) => {
      route.data = { permissions: ['user.edit', 'user.delete'] };
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');
      authServiceSpy.hasAnyPermission.and.returnValue(true);

      TestBed.runInInjectionContext(() => {
        const result = permissionGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(authServiceSpy.hasAnyPermission).toHaveBeenCalledWith(['user.edit', 'user.delete']);
          done();
        });
      });
    });
  });
});
