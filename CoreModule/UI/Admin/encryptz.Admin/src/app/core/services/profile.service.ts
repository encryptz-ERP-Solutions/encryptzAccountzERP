import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserProfile {
  userId: string;
  userHandle: string;
  fullName?: string;
  email?: string;
  hasPanCard: boolean;
  isProfileComplete: boolean;
}

export interface UpdateUserProfileRequest {
  fullName: string;
  panCardNumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly baseUrl = `${environment.apiUrl}api/Profile`;

  constructor(private http: HttpClient) { }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`, {
      withCredentials: true
    });
  }

  updateProfile(payload: UpdateUserProfileRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/me`, payload, {
      withCredentials: true
    });
  }
}

