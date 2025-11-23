import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { CommonService } from '../../../../shared/services/common.service';
import { AccountsSideBarComponent } from './accounts-side-bar.component';

class CommonServiceStub {
  showSnackbar() { }
}

describe('AccountsSideBarComponent', () => {
  let component: AccountsSideBarComponent;
  let fixture: ComponentFixture<AccountsSideBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsSideBarComponent, RouterTestingModule],
      providers: [{ provide: CommonService, useClass: CommonServiceStub }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountsSideBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
