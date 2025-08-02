import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {

  constructor(
    private http : HttpClient
  ){}


  createNewUser(body : any){
    return this.http.post(environment.apiUrl +  'api/User', body)
  }

  getAllUser(){
    return this.http.get(environment.apiUrl +  'api/User')
  }
  
}
