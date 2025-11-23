import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileService } from '../../../core/services/profile.service';
import { CommonService } from '../../../shared/services/common.service';
import { ProfileSetupComponent } from './profile-setup.component';

class AuthServiceStub {
  isAuthenticated() {
    return true;
  }
  refresh() {
    return of(null);
  }
}

class ProfileServiceStub {
  getProfile() {
    return of({
      userId: '1',
      userHandle: 'demo',
      hasPanCard: false,
      isProfileComplete: false
    });
  }
  updateProfile() {
    return of(null);
  }
}

class CommonServiceStub {
  showSnackbar() { }
}

describe('ProfileSetupComponent', () => {
  let component: ProfileSetupComponent;
  let fixture: ComponentFixture<ProfileSetupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileSetupComponent, RouterTestingModule],
      providers: [
        { provide: AuthService, useClass: AuthServiceStub },
        { provide: ProfileService, useClass: ProfileServiceStub },
        { provide: CommonService, useClass: CommonServiceStub }
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ProfileSetupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

