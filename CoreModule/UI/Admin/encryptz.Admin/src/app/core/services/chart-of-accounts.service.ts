import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ChartOfAccount {
  accountID: string;
  businessID: string;
  accountTypeID: number;
  parentAccountID?: string | null;
  accountCode: string;
  accountName: string;
  description?: string | null;
  isActive: boolean;
  isSystemAccount: boolean;
  createdAtUTC: string;
  updatedAtUTC?: string | null;
  accountTypeName?: string;
  parentAccountName?: string;
}

export interface AccountType {
  accountTypeID: number;
  accountTypeName: string;
  normalBalance: string;
}

export interface CreateChartOfAccountRequest {
  businessID: string;
  accountTypeID: number;
  parentAccountID?: string | null;
  accountCode: string;
  accountName: string;
  description?: string | null;
}

export interface UpdateChartOfAccountRequest {
  accountName: string;
  description?: string | null;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ChartOfAccountsService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getAll(businessId: string): Observable<ChartOfAccount[]> {
    // Ensure businessId is properly formatted (remove any whitespace)
    const cleanBusinessId = businessId?.trim();
    if (!cleanBusinessId) {
      throw new Error('Business ID is required');
    }
    console.log('ChartOfAccountsService.getAll - calling API with businessId:', cleanBusinessId);
    const url = `${this.baseUrl}api/ChartOfAccounts/business/${cleanBusinessId}`;
    console.log('Full URL:', url);
    return this.http.get<ChartOfAccount[]>(url);
  }

  getById(id: string): Observable<ChartOfAccount> {
    return this.http.get<ChartOfAccount>(`${this.baseUrl}api/ChartOfAccounts/${id}`);
  }

  create(payload: CreateChartOfAccountRequest): Observable<ChartOfAccount> {
    return this.http.post<ChartOfAccount>(`${this.baseUrl}api/ChartOfAccounts`, payload);
  }

  update(id: string, payload: UpdateChartOfAccountRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/ChartOfAccounts/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/ChartOfAccounts/${id}`);
  }

  getAccountTypes(): Observable<AccountType[]> {
    return this.http.get<AccountType[]>(`${this.baseUrl}api/AccountTypes`);
  }
}

