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

  // updateUser(body : any){
  //   return this.http.put(environment.apiUrl +  'api/User', body)
  // }
  updateUser(id: number, body: any) {
    return this.http.put(`${environment.apiUrl}api/User/${id}`, body);
  }

  getAllUser(){
    return this.http.get(environment.apiUrl +  'api/User')
  }
  
}
