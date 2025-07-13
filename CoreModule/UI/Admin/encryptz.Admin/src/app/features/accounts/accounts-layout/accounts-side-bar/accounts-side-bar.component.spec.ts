import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountsSideBarComponent } from './accounts-side-bar.component';

describe('AccountsSideBarComponent', () => {
  let component: AccountsSideBarComponent;
  let fixture: ComponentFixture<AccountsSideBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsSideBarComponent]
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
