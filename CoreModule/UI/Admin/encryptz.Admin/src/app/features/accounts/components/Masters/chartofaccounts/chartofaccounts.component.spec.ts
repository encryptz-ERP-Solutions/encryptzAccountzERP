import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartofaccountsComponent } from './chartofaccounts.component';

describe('ChartofaccountsComponent', () => {
  let component: ChartofaccountsComponent;
  let fixture: ComponentFixture<ChartofaccountsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartofaccountsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChartofaccountsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
