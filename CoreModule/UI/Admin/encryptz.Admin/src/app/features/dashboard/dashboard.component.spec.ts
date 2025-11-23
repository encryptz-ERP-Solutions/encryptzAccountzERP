import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BusinessContextService } from '../../core/services/business-context.service';
import { UserBusinessService } from '../../core/services/user-business.service';
import { CommonService } from '../../shared/services/common.service';
import { DashboardComponent } from './dashboard.component';

class AuthServiceStub {
  getUser() {
    return { userId: 'user-1', userName: 'Test User', userHandle: 'testUser' };
  }

  isProfileComplete() {
    return true;
  }

  isSystemAdmin() {
    return false;
  }
}

class UserBusinessServiceStub {
  getUserBusinesses() {
    return of([]);
  }
}

class BusinessContextServiceStub {
  selectedBusiness$ = of(null);
  currentBusiness = null;
  clearBusinessContext() { }
  setSelectedBusiness() { }
  setSelectedModule() { }
}

class CommonServiceStub {
  showSnackbar() { }
}

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent, RouterTestingModule],
      providers: [
        { provide: AuthService, useClass: AuthServiceStub },
        { provide: UserBusinessService, useClass: UserBusinessServiceStub },
        { provide: BusinessContextService, useClass: BusinessContextServiceStub },
        { provide: CommonService, useClass: CommonServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
