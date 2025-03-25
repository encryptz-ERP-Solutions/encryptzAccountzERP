import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommonService {

  constructor(private http: HttpClient) { }

  private apiUrl = 'assets/states.json';
  private uqcUrl = 'assets/uqc.json';

  eventEmitItems = new EventEmitter<{ items: any[], bankInfo: any }>();
  eventEmit = new EventEmitter<string>();
  
  getStates(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  getUqc(): Observable<any[]> {
    return this.http.get<any[]>(this.uqcUrl);
  }
  
  PassToAnotherComp(data: string) {
    this.eventEmit.emit(data);
  }
  
  passItems(items: any[], bankInfo: any) {
    this.eventEmitItems.emit({ items, bankInfo });
  }

  
}
