import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileService, UpdateUserProfileRequest, UserProfile } from '../../../core/services/profile.service';
import { CommonService } from '../../../shared/services/common.service';

@Component({
  selector: 'app-profile-setup',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './profile-setup.component.html',
  styleUrl: './profile-setup.component.scss'
})
export class ProfileSetupComponent implements OnInit {
  profile?: UserProfile;
  isLoading = false;
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private authService: AuthService,
    private commonService: CommonService,
    private router: Router
  ) {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
      panCardNumber: ['', [Validators.required, Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/i)]]
    });
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return;
    }

    this.loadProfile();
  }

  private loadProfile(): void {
    this.isLoading = true;
    this.profileService.getProfile()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: profile => {
          this.profile = profile;
          this.form.patchValue({
            fullName: profile.fullName ?? '',
            panCardNumber: ''
          });
        },
        error: err => {
          const message = err?.error?.message || 'Unable to load profile details.';
          this.commonService.showSnackbar(message, 'ERROR', 3000);
        }
      });
  }

  submit(): void {
    if (this.form.invalid || this.isLoading) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: UpdateUserProfileRequest = {
      fullName: this.form.value.fullName!.trim(),
      panCardNumber: this.form.value.panCardNumber!.toUpperCase()
    };

    this.isLoading = true;
    this.profileService.updateProfile(payload)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          this.commonService.showSnackbar('Profile updated successfully.', 'SUCCESS', 3000);
          this.authService.refresh().subscribe({
            next: () => this.router.navigate(['/dashboard']),
            error: () => this.router.navigate(['/dashboard'])
          });
        },
        error: err => {
          const message = err?.error?.message || 'Unable to update profile.';
          this.commonService.showSnackbar(message, 'ERROR', 3000);
        }
      });
  }
}

