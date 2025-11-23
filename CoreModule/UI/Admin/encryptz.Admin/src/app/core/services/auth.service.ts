import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

// Interfaces
export interface User {
  userId: string;
  email: string;
  userName: string;
  userHandle?: string;
  role: string;
  businessId?: string;
  permissions?: string[];
  exp?: number;
  isSystemAdmin?: boolean;
  isProfileComplete?: boolean;
}

export interface LoginCredentials {
  emailOrUserHandle: string;
  password: string;
  fullName?: string;
  panCardNumber?: string;
}

export interface AuthTokensResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  userHandle: string;
  isProfileComplete: boolean;
  isSystemAdmin: boolean;
  refreshToken?: string | null;
  refreshTokenExpiresAt?: string | null;
}

export type RefreshResponse = AuthTokensResponse;

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // In-memory access token storage (more secure than localStorage)
  private accessToken: string | null = null;
  
  // Observables for reactive state management
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();
  
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  public isLoggedIn$: Observable<boolean> = this.isLoggedInSubject.asObservable();
  
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private refreshTokenValue: string | null = null;
  private refreshTokenExpiresAt: string | null = null;

  private readonly refreshTokenStorageKey = 'encryptz_refresh_token';
  private readonly refreshTokenExpiryStorageKey = 'encryptz_refresh_token_expires_at';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.restorePersistedRefreshToken();
    // Try to restore session on init (in case page was refreshed)
    this.initializeSession();
  }

  /**
   * Initialize session by attempting to refresh token
   * This handles page refresh scenarios
   */
  private initializeSession(): void {
    if (!this.refreshTokenValue) {
      return;
    }

    // Try to refresh token on app startup (works if httpOnly cookie exists)
    this.refresh().subscribe({
      next: () => {
        console.log('Session restored from refresh token');
      },
      error: () => {
        // No valid refresh token, user needs to login
        this.clearSession();
      }
    });
  }

  /**
   * Login with credentials
   * Backend should set httpOnly cookie with refresh token
   */
  login(credentials: LoginCredentials): Observable<AuthTokensResponse> {
    return this.http.post<AuthTokensResponse>(
      `${environment.apiUrl}api/v1/auth/login`,
      credentials,
      { withCredentials: true } // Important: allows cookies to be sent/received
    ).pipe(
      tap(response => {
        if (response?.accessToken) {
          this.setSession(response.accessToken);
        }
        if (response?.refreshToken) {
          this.setRefreshTokenValue(response.refreshToken, response.refreshTokenExpiresAt ?? null);
        }
      }),
      catchError(error => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Logout - clear local session and inform backend to clear refresh token cookie
   */
  logout(): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}api/v1/auth/logout`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() => this.clearSession()),
      catchError(error => {
        // Even if backend call fails, clear local session
        this.clearSession();
        return of(null);
      })
    );
  }

  /**
   * Refresh access token using httpOnly cookie
   * This is called automatically on 401 responses
   */
  refresh(): Observable<RefreshResponse> {
    const payload: { refreshToken?: string } = {};
    if (this.refreshTokenValue) {
      payload.refreshToken = this.refreshTokenValue;
    }

    return this.http.post<RefreshResponse>(
      `${environment.apiUrl}api/v1/auth/refresh`,
      payload,
      { withCredentials: true } // Important: sends httpOnly cookie / fallback to body
    ).pipe(
      tap(response => {
        if (response?.accessToken) {
          this.setAccessToken(response.accessToken);
        }
        if (response?.refreshToken) {
          this.setRefreshTokenValue(response.refreshToken, response.refreshTokenExpiresAt ?? null);
        }
      }),
      catchError(error => {
        this.clearSession();
        return throwError(() => error);
      })
    );
  }

  /**
   * Get current access token
   */
  getAccessToken(): string | null {
    return this.accessToken;
  }

  /**
   * Get current user
   */
  getUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.isLoggedInSubject.value && this.accessToken !== null;
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.role === role;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this.getUser();
    return user?.role ? roles.includes(user.role) : false;
  }

  /**
   * Check if user has specific permission
   */
  hasPermission(permission: string): boolean {
    const user = this.getUser();
    return user?.permissions?.includes(permission) ?? false;
  }

  /**
   * Check if user has any of the specified permissions
   */
  hasAnyPermission(permissions: string[]): boolean {
    const user = this.getUser();
    return user?.permissions 
      ? permissions.some(p => user.permissions!.includes(p)) 
      : false;
  }

  isSystemAdmin(): boolean {
    return !!this.getUser()?.isSystemAdmin;
  }

  isProfileComplete(): boolean {
    return !!this.getUser()?.isProfileComplete;
  }

  /**
   * Set access token and decode user info
   */
  private setAccessToken(token: string): void {
    this.accessToken = token;
    const user = this.decodeToken(token);
    if (user) {
      this.currentUserSubject.next(user);
      this.isLoggedInSubject.next(true);
    }
  }

  /**
   * Set session after successful login
   */
  private setSession(token: string): void {
    this.setAccessToken(token);
  }

  /**
   * Clear session data
   */
  clearSession(): void {
    this.accessToken = null;
    this.currentUserSubject.next(null);
    this.isLoggedInSubject.next(false);
    this.isRefreshing = false;
    this.refreshTokenSubject.next(null);
    this.setRefreshTokenValue(null);
  }

  /**
   * Decode JWT token to extract user information
   */
  private decodeToken(token: string): User | null {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      
      // Map JWT claims to User interface
      // Adjust these mappings based on your backend JWT structure
      return {
        userId: decoded.sub || decoded.userId || decoded.id,
        email: decoded.email,
        userName: decoded.name || decoded.userName || decoded.unique_name || decoded.userHandle,
        userHandle: decoded.name || decoded.userHandle,
        role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
        businessId: decoded.businessId,
        permissions: decoded.permissions || [],
        exp: decoded.exp,
        isSystemAdmin: decoded['is_admin'] === 'true',
        isProfileComplete: decoded['profile_complete'] !== 'false'
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Check if token is expired
   */
  isTokenExpired(): boolean {
    const user = this.getUser();
    if (!user?.exp) return true;
    
    const expiryTime = user.exp * 1000; // Convert to milliseconds
    return Date.now() >= expiryTime;
  }

  /**
   * Get token expiry time in milliseconds
   */
  getTokenExpiryTime(): number | null {
    const user = this.getUser();
    return user?.exp ? user.exp * 1000 : null;
  }

  // ============= OTP Methods (keeping existing functionality) =============
  
  requestOTP(body: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}api/Login/request-otp`, body);
  }

  requestForgotOTP(body: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}api/Login/forgot-password`, body);
  }

  verifyOTP(body: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}api/Login/verify-otp`, body, { withCredentials: true })
      .pipe(
        tap((res: any) => {
          if (res.isSuccess && res.token) {
            this.setSession(res.token);
          } else if (res.isSuccess && res.response?.token) {
            this.setSession(res.response.token);
          }
        })
      );
  }

  loginWithLoginController(body: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}api/Login/login`, body, { withCredentials: true })
      .pipe(
        tap((res: any) => {
          if (res.isSuccess && res.token) {
            this.setSession(res.token);
          }
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  resetPassword(body: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}api/Login/reset-password`, body, { withCredentials: true })
      .pipe(
        tap((res: any) => {
          if (res.status && res.token) {
            this.setSession(res.token);
          }
        })
      );
  }

  // ============= Refresh Token Management =============
  
  /**
   * Get refresh observable for handling concurrent refresh requests
   */
  getRefreshTokenObservable(): Observable<string | null> {
    return this.refreshTokenSubject.asObservable();
  }

  /**
   * Set refreshing state
   */
  setRefreshing(refreshing: boolean): void {
    this.isRefreshing = refreshing;
  }

  /**
   * Check if currently refreshing token
   */
  isRefreshingToken(): boolean {
    return this.isRefreshing;
  }

  /**
   * Set refresh token subject
   */
  setRefreshTokenSubject(token: string | null): void {
    this.refreshTokenSubject.next(token);
  }

  private setRefreshTokenValue(token: string | null, expiresAt: string | null = null): void {
    this.refreshTokenValue = token;
    this.refreshTokenSubject.next(token);
    this.refreshTokenExpiresAt = expiresAt;

    try {
      if (token) {
        localStorage.setItem(this.refreshTokenStorageKey, token);
        if (expiresAt) {
          localStorage.setItem(this.refreshTokenExpiryStorageKey, expiresAt);
        } else {
          localStorage.removeItem(this.refreshTokenExpiryStorageKey);
        }
      } else {
        localStorage.removeItem(this.refreshTokenStorageKey);
        localStorage.removeItem(this.refreshTokenExpiryStorageKey);
      }
    } catch (error) {
      console.warn('Unable to persist refresh token', error);
    }
  }

  private restorePersistedRefreshToken(): void {
    try {
      this.refreshTokenValue = localStorage.getItem(this.refreshTokenStorageKey);
      this.refreshTokenExpiresAt = localStorage.getItem(this.refreshTokenExpiryStorageKey);
      if (this.refreshTokenValue) {
        this.refreshTokenSubject.next(this.refreshTokenValue);
      }
    } catch {
      this.refreshTokenValue = null;
      this.refreshTokenExpiresAt = null;
    }
  }
}
