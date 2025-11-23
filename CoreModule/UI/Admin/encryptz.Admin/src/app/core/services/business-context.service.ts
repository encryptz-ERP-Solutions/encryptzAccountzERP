import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { UserBusinessSummary } from './user-business.service';

export interface SelectedBusiness {
  userBusinessID: string;
  businessID: string;
  businessName?: string | null;
  businessCode?: string | null;
  isDefault: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class BusinessContextService {
  private readonly selectedBusinessKey = 'encryptz:selectedBusiness';
  private readonly selectedModuleKey = 'encryptz:selectedModule';

  private selectedBusinessSubject = new BehaviorSubject<SelectedBusiness | null>(this.restoreBusiness());
  private selectedModuleSubject = new BehaviorSubject<string | null>(this.restoreModule());

  selectedBusiness$: Observable<SelectedBusiness | null> = this.selectedBusinessSubject.asObservable();
  selectedModule$: Observable<string | null> = this.selectedModuleSubject.asObservable();

  get currentBusiness(): SelectedBusiness | null {
    return this.selectedBusinessSubject.value;
  }

  get currentModule(): string | null {
    return this.selectedModuleSubject.value;
  }

  setSelectedBusiness(business: SelectedBusiness | UserBusinessSummary | null): void {
    if (business) {
      const normalized: SelectedBusiness = {
        userBusinessID: business.userBusinessID,
        businessID: business.businessID,
        businessName: business.businessName,
        businessCode: business.businessCode,
        isDefault: business.isDefault
      };
      this.selectedBusinessSubject.next(normalized);
      localStorage.setItem(this.selectedBusinessKey, JSON.stringify(normalized));
    } else {
      this.selectedBusinessSubject.next(null);
      localStorage.removeItem(this.selectedBusinessKey);
    }
  }

  setSelectedModule(moduleKey: string | null): void {
    this.selectedModuleSubject.next(moduleKey);
    if (moduleKey) {
      localStorage.setItem(this.selectedModuleKey, moduleKey);
    } else {
      localStorage.removeItem(this.selectedModuleKey);
    }
  }

  clearBusinessContext(): void {
    this.setSelectedBusiness(null);
    this.setSelectedModule(null);
  }

  private restoreBusiness(): SelectedBusiness | null {
    const stored = localStorage.getItem(this.selectedBusinessKey);
    if (!stored) {
      return null;
    }
    try {
      return JSON.parse(stored) as SelectedBusiness;
    } catch {
      localStorage.removeItem(this.selectedBusinessKey);
      return null;
    }
  }

  private restoreModule(): string | null {
    return localStorage.getItem(this.selectedModuleKey);
  }
}

