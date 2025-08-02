import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  permissions: string[];
}

export interface AuthResponse {
  token: string;
  user: User;
  refreshToken?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient
  ) {
    this.loadUserFromStorage();
  }

  // Token Management
  setToken(token: string): void {
    localStorage.setItem('access_token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  removeToken(): void {
    localStorage.removeItem('access_token');
  }


  // User Management
  setCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
    localStorage.setItem('current_user', JSON.stringify(user));
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  removeCurrentUser(): void {
    this.currentUserSubject.next(null);
    localStorage.removeItem('current_user');
  }

  private loadUserFromStorage(): void {
    const userStr = localStorage.getItem('current_user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error parsing user from storage:', error);
        this.logout();
      }
    }
  }

  sendOTP(body: any) {
    return this.http.post(environment.apiUrl + 'api/Login/send-otp', body)
  }

  verifyOTP(body: any) {
    return this.http.post(environment.apiUrl + 'api/Login/verify-otp', body)
      .pipe(
        tap((res: any) => {
          debugger
          this.setToken(res.response.token);
          // this.setCurrentUser(res.response.user);
        })
      );
  }

  login(credentials: any) {
    return this.http.post(environment.apiUrl + 'api/Login/login', credentials)
      .pipe(
        tap((res: any) => {
          this.setToken(res.response.token);
          // this.setCurrentUser(response.user);
        })
      );
  }

  logout(): void {
    this.removeToken();
    this.removeCurrentUser();
  }


  // Permission Methods
  isAuthenticated(): boolean {
    return !!this.getToken() && !!this.getCurrentUser();
  }

  // hasRole(role: string): boolean {
  //   const user = this.getCurrentUser();
  //   return user ? user.role === role : false;
  // }

  // hasPermission(permission: string): boolean {
  //   const user = this.getCurrentUser();
  //   return user ? user.permissions.includes(permission) : false;
  // }

  // hasAnyPermission(permissions: string[]): boolean {
  //   const user = this.getCurrentUser();
  //   return user ? permissions.some(permission => user.permissions.includes(permission)) : false;
  // }

  // Token Refresh
  // refreshToken(): Observable<AuthResponse> {
  //   return this.http.post<AuthResponse>(environment.apiUrl + 'api/Login/refresh-token', {})
  //     .pipe(
  //       tap(response => {
  //         this.setToken(response.token);
  //         this.setCurrentUser(response.user);
  //       })
  //     );
  // }

  // isTokenExpired(): boolean {
  //   const token = this.getToken();
  //   if (!token) return true;

  //   try {
  //     const payload = JSON.parse(atob(token.split('.')[1]));
  //     const expiry = payload.exp * 1000; // Convert to milliseconds
  //     return Date.now() >= expiry;
  //   } catch (error) {
  //     return true;
  //   }
  // }

}
