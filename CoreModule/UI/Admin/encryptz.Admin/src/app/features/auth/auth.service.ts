import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(
    private http: HttpClient
  ) { }

  sendOTP(body: any) {
    return this.http.post(environment.apiUrl + 'api/Login/send-otp', body)
  }

  verifyOTP(body: any) {
    return this.http.post(environment.apiUrl + 'api/Login/verify-otp', body)
  }

}
