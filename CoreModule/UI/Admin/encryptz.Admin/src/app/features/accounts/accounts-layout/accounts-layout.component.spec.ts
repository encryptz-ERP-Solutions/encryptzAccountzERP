import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { BusinessContextService } from '../../../core/services/business-context.service';
import { CommonService } from '../../../shared/services/common.service';
import { AuthService } from '../../../core/services/auth.service';
import { AccountsLayoutComponent } from './accounts-layout.component';

class BusinessContextServiceStub {
  selectedBusiness$ = of({
    userBusinessID: '1',
    businessID: '2',
    businessName: 'Demo Business',
    businessCode: 'DEM001',
    isDefault: true
  });
  currentBusiness = null;
  clearBusinessContext() { }
}

class CommonServiceStub {
  showSnackbar() { }
}

class AuthServiceStub {
  isProfileComplete() {
    return true;
  }
}

describe('AccountsLayoutComponent', () => {
  let component: AccountsLayoutComponent;
  let fixture: ComponentFixture<AccountsLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsLayoutComponent, RouterTestingModule],
      providers: [
        { provide: BusinessContextService, useClass: BusinessContextServiceStub },
        { provide: CommonService, useClass: CommonServiceStub },
        { provide: AuthService, useClass: AuthServiceStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountsLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
