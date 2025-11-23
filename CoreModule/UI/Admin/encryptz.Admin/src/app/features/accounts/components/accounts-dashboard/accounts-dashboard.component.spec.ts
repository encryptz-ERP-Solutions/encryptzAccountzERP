import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { BusinessContextService } from '../../../../core/services/business-context.service';
import { CommonService } from '../../../../shared/services/common.service';
import { AccountsDashboardComponent } from './accounts-dashboard.component';

class BusinessContextServiceStub {
  selectedBusiness$ = of(null);
}

class CommonServiceStub {
  showSnackbar() { }
}

describe('AccountsDashboardComponent', () => {
  let component: AccountsDashboardComponent;
  let fixture: ComponentFixture<AccountsDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsDashboardComponent, RouterTestingModule],
      providers: [
        { provide: BusinessContextService, useClass: BusinessContextServiceStub },
        { provide: CommonService, useClass: CommonServiceStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountsDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
