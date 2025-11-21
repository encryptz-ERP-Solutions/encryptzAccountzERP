import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard, guestGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { of } from 'rxjs';

describe('AuthGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let route: ActivatedRouteSnapshot;
  let state: RouterStateSnapshot;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', ['getAccessToken'], {
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
    
    route = {} as ActivatedRouteSnapshot;
    state = { url: '/dashboard' } as RouterStateSnapshot;
  });

  describe('authGuard', () => {
    it('should allow access when user is authenticated', (done) => {
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');

      TestBed.runInInjectionContext(() => {
        const result = authGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(routerSpy.navigate).not.toHaveBeenCalled();
          done();
        });
      });
    });

    it('should deny access and redirect to login when user is not authenticated', (done) => {
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(false) });
      authServiceSpy.getAccessToken.and.returnValue(null);

      TestBed.runInInjectionContext(() => {
        const result = authGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(false);
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/'], {
            queryParams: { returnUrl: '/dashboard' }
          });
          done();
        });
      });
    });

    it('should preserve return URL in query params', (done) => {
      const protectedUrl = '/admin/users';
      state = { url: protectedUrl } as RouterStateSnapshot;
      
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(false) });
      authServiceSpy.getAccessToken.and.returnValue(null);

      TestBed.runInInjectionContext(() => {
        const result = authGuard(route, state);
        result.subscribe(() => {
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/'], {
            queryParams: { returnUrl: protectedUrl }
          });
          done();
        });
      });
    });
  });

  describe('guestGuard', () => {
    it('should allow access when user is not authenticated', (done) => {
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(false) });
      authServiceSpy.getAccessToken.and.returnValue(null);

      TestBed.runInInjectionContext(() => {
        const result = guestGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(true);
          expect(routerSpy.navigate).not.toHaveBeenCalled();
          done();
        });
      });
    });

    it('should deny access and redirect to dashboard when user is authenticated', (done) => {
      Object.defineProperty(authServiceSpy, 'isLoggedIn$', { value: of(true) });
      authServiceSpy.getAccessToken.and.returnValue('test-token');

      TestBed.runInInjectionContext(() => {
        const result = guestGuard(route, state);
        result.subscribe(canActivate => {
          expect(canActivate).toBe(false);
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard']);
          done();
        });
      });
    });
  });
});
