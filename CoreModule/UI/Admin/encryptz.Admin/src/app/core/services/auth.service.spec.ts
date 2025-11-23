import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService, AuthTokensResponse, LoginCredentials, RefreshResponse } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;
  let routerSpyObj: jasmine.SpyObj<Router>;

  const initService = () => {
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  };

  beforeEach(() => {
    localStorage.clear();
    routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: routerSpyObj }
      ]
    });
  });

  afterEach(() => {
    if (httpMock) {
      httpMock.verify();
    }
  });

  it('should be created', () => {
    initService();
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login successfully and set session', (done) => {
      initService();

      const credentials: LoginCredentials = {
        emailOrUserHandle: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthTokensResponse = {
        accessToken: 'test-access-token',
        expiresAt: new Date().toISOString(),
        userId: 'user-123',
        userHandle: 'testUser',
        isProfileComplete: true,
        isSystemAdmin: false,
        refreshToken: 'refresh-token-123',
        refreshTokenExpiresAt: new Date().toISOString()
      };

      service.login(credentials).subscribe(response => {
        expect(response.accessToken).toBe(mockResponse.accessToken);
        expect(service.getAccessToken()).toBeTruthy();
        expect(service.isAuthenticated()).toBe(true);
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockResponse);
    });

    it('should handle login failure', (done) => {
      initService();

      const credentials: LoginCredentials = {
        emailOrUserHandle: 'test@example.com',
        password: 'wrongpassword'
      };

      const mockError = {
        isSuccess: false,
        message: 'Invalid credentials'
      };

      service.login(credentials).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.error.message).toBe('Invalid credentials');
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/login`);
      req.flush(mockError, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('logout', () => {
    it('should clear session on logout', (done) => {
      initService();

      service.logout().subscribe(() => {
        expect(service.getAccessToken()).toBeNull();
        expect(service.getUser()).toBeNull();
        expect(service.isAuthenticated()).toBe(false);
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/logout`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({});
    });

    it('should clear session even if logout request fails', (done) => {
      initService();

      service.logout().subscribe(() => {
        expect(service.getAccessToken()).toBeNull();
        expect(service.isAuthenticated()).toBe(false);
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/logout`);
      req.error(new ProgressEvent('error'));
    });
  });

  describe('refresh', () => {
    it('should refresh token successfully', (done) => {
      initService();

      const mockResponse: RefreshResponse = {
        accessToken: 'new-token-here',
        expiresAt: new Date().toISOString(),
        userId: 'user-123',
        userHandle: 'testUser',
        isProfileComplete: true,
        isSystemAdmin: false,
        refreshToken: 'new-refresh-token',
        refreshTokenExpiresAt: new Date().toISOString()
      };

      service.refresh().subscribe(response => {
        expect(response.accessToken).toBe('new-token-here');
        expect(service.getAccessToken()).toBe('new-token-here');
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockResponse);
    });

    it('should send stored refresh token when available', () => {
      initService();

      const future = new Date(Date.now() + 3600 * 1000).toISOString();
      // @ts-ignore
      service['setRefreshTokenValue']('stored-refresh-token', future);

      service.refresh().subscribe({
        next: () => { },
        error: () => fail('refresh should not fail')
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      expect(req.request.body).toEqual({ refreshToken: 'stored-refresh-token' });
      req.flush({
        accessToken: 'another-token',
        expiresAt: new Date().toISOString(),
        userId: 'user-123',
        userHandle: 'testUser',
        isProfileComplete: true,
        isSystemAdmin: false,
        refreshToken: 'next-refresh-token',
        refreshTokenExpiresAt: future
      } as RefreshResponse);
    });

    it('should clear session if refresh fails', (done) => {
      initService();

      service.refresh().subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(service.getAccessToken()).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      req.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('should trigger refresh on init when persisted token exists', () => {
      const future = new Date(Date.now() + 3600 * 1000).toISOString();
      localStorage.setItem('encryptz_refresh_token', 'persisted-token');
      localStorage.setItem('encryptz_refresh_token_expires_at', future);

      initService();

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: 'persisted-token' });
      req.flush({
        accessToken: 'restored-token',
        expiresAt: future,
        userId: 'user-123',
        userHandle: 'testUser',
        isProfileComplete: true,
        isSystemAdmin: false,
        refreshToken: 'next-persisted-token',
        refreshTokenExpiresAt: future
      } as RefreshResponse);
    });

    it('should not refresh on init when no persisted token', () => {
      initService();
      httpMock.expectNone(`${environment.apiUrl}api/v1/auth/refresh`);
    });
  });

  describe('getAccessToken', () => {
    it('should return null when not authenticated', () => {
      initService();
      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when not authenticated', () => {
      initService();
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('hasRole', () => {
    it('should return false when user has different role', () => {
      initService();
      expect(service.hasRole('Admin')).toBe(false);
    });

    it('should check for specific role', () => {
      initService();
      expect(service.hasRole('User')).toBe(false);
    });
  });

  describe('hasAnyRole', () => {
    it('should return false when user has no matching roles', () => {
      initService();
      expect(service.hasAnyRole(['Admin', 'Manager'])).toBe(false);
    });
  });

  describe('hasPermission', () => {
    it('should return false when user has no permissions', () => {
      initService();
      expect(service.hasPermission('user.edit')).toBe(false);
    });
  });

  describe('hasAnyPermission', () => {
    it('should return false when user has no matching permissions', () => {
      initService();
      expect(service.hasAnyPermission(['user.edit', 'user.delete'])).toBe(false);
    });
  });

  describe('currentUser$ observable', () => {
    it('should emit null initially', (done) => {
      initService();
      service.currentUser$.subscribe(user => {
        expect(user).toBeNull();
        done();
      });
    });
  });

  describe('isLoggedIn$ observable', () => {
    it('should emit false initially', (done) => {
      initService();
      service.isLoggedIn$.subscribe(isLoggedIn => {
        expect(isLoggedIn).toBe(false);
        done();
      });
    });
  });

  describe('OTP methods', () => {
    it('should request OTP', (done) => {
      initService();
      const payload = { loginIdentifier: 'test@example.com', otpMethod: 'email' };

      service.requestOTP(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/request-otp`);
      expect(req.request.method).toBe('POST');
      req.flush({ status: true });
    });

    it('should verify OTP', (done) => {
      initService();
      const payload = { loginIdentifier: 'test@example.com', otp: '123456' };

      service.verifyOTP(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/verify-otp`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ isSuccess: true, response: { token: 'test-token' } });
    });

    it('should request forgot password OTP', (done) => {
      initService();
      const payload = { loginIdentifier: 'test@example.com' };

      service.requestForgotOTP(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/forgot-password`);
      expect(req.request.method).toBe('POST');
      req.flush({ status: true });
    });

    it('should reset password', (done) => {
      initService();
      const payload = { loginIdentifier: 'test@example.com', otp: '123456', newPassword: 'newpass' };

      service.resetPassword(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/reset-password`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush({ status: true, token: 'test-token' });
    });
  });
});
