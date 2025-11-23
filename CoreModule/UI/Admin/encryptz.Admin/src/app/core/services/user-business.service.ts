import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserBusinessSummary {
  userBusinessID: string;
  userID: string;
  businessID: string;
  businessName?: string | null;
  businessCode?: string | null;
  isDefault: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserBusinessService {
  constructor(private http: HttpClient) { }

  getUserBusinesses(userId: string): Observable<UserBusinessSummary[]> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<UserBusinessSummary[]>(`${environment.apiUrl}api/UserBusinesses`, {
      params,
      withCredentials: true
    });
  }
}

