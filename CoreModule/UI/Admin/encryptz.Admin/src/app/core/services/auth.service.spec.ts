import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService, LoginCredentials, LoginResponse, RefreshResponse, User } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login successfully and set session', (done) => {
      const credentials: LoginCredentials = {
        loginIdentifier: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: LoginResponse = {
        isSuccess: true,
        message: 'Login successful',
        response: {
          token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgVXNlciIsInJvbGUiOiJVc2VyIiwiZXhwIjo5OTk5OTk5OTk5fQ.X'
        }
      };

      service.login(credentials).subscribe(response => {
        expect(response.isSuccess).toBe(true);
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
      const credentials: LoginCredentials = {
        loginIdentifier: 'test@example.com',
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
      const mockResponse: RefreshResponse = {
        isSuccess: true,
        response: {
          token: 'new-token-here'
        }
      };

      service.refresh().subscribe(response => {
        expect(response.isSuccess).toBe(true);
        expect(service.getAccessToken()).toBe('new-token-here');
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockResponse);
    });

    it('should clear session if refresh fails', (done) => {
      service.refresh().subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(service.getAccessToken()).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/v1/auth/refresh`);
      req.flush({ isSuccess: false }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('getAccessToken', () => {
    it('should return null when not authenticated', () => {
      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when not authenticated', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('hasRole', () => {
    it('should return false when user has different role', () => {
      expect(service.hasRole('Admin')).toBe(false);
    });

    it('should check for specific role', () => {
      // Note: This would need a properly authenticated user to test properly
      expect(service.hasRole('User')).toBe(false);
    });
  });

  describe('hasAnyRole', () => {
    it('should return false when user has no matching roles', () => {
      expect(service.hasAnyRole(['Admin', 'Manager'])).toBe(false);
    });
  });

  describe('hasPermission', () => {
    it('should return false when user has no permissions', () => {
      expect(service.hasPermission('user.edit')).toBe(false);
    });
  });

  describe('hasAnyPermission', () => {
    it('should return false when user has no matching permissions', () => {
      expect(service.hasAnyPermission(['user.edit', 'user.delete'])).toBe(false);
    });
  });

  describe('currentUser$ observable', () => {
    it('should emit null initially', (done) => {
      service.currentUser$.subscribe(user => {
        expect(user).toBeNull();
        done();
      });
    });
  });

  describe('isLoggedIn$ observable', () => {
    it('should emit false initially', (done) => {
      service.isLoggedIn$.subscribe(isLoggedIn => {
        expect(isLoggedIn).toBe(false);
        done();
      });
    });
  });

  describe('OTP methods', () => {
    it('should request OTP', (done) => {
      const payload = { loginIdentifier: 'test@example.com', otpMethod: 'email' };

      service.requestOTP(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/request-otp`);
      expect(req.request.method).toBe('POST');
      req.flush({ status: true });
    });

    it('should verify OTP', (done) => {
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
      const payload = { loginIdentifier: 'test@example.com' };

      service.requestForgotOTP(payload).subscribe(() => {
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}api/Login/forgot-password`);
      expect(req.request.method).toBe('POST');
      req.flush({ status: true });
    });

    it('should reset password', (done) => {
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
