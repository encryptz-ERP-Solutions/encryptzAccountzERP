import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../../core/services/admin-data.service';
import { CommonService } from '../../../../../shared/services/common.service';
import { AdminUser, CreateAdminUserRequest, UpdateAdminUserRequest } from '../../../../../core/models/admin.models';

@Component({
  selector: 'app-add-edit-user',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    ReactiveFormsModule
  ],
  templateUrl: './add-edit-user.component.html',
  styleUrl: './add-edit-user.component.scss'
})
export class AddEditUserComponent {
  form!: FormGroup;
  mode: 'create' | 'edit' = 'create';
  submitting = false;

  constructor(
    private dialogRef: MatDialogRef<AddEditUserComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit', user?: AdminUser },
    private fb: FormBuilder,
    private adminDataService: AdminDataService,
    private commonService: CommonService,
  ) {
    this.dialogRef.disableClose = true;
  }


  ngOnInit() {
    this.mode = this.data?.mode ?? 'create';
    this.buildForm();

    if (this.mode === 'edit' && this.data.user) {
      this.form.patchValue({
        userHandle: this.data.user.userHandle,
        fullName: this.data.user.fullName,
        email: this.data.user.email,
        mobileCountryCode: this.data.user.mobileCountryCode,
        mobileNumber: this.data.user.mobileNumber,
        isActive: this.data.user.isActive
      });
      this.form.get('userHandle')?.disable();
    }
  }

  private buildForm(): void {
    this.form = this.fb.group({
      userHandle: ['', [Validators.required, Validators.minLength(3)]],
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', this.mode === 'create' ? [Validators.required, Validators.minLength(8)] : []],
      mobileCountryCode: ['+91'],
      mobileNumber: [''],
      isActive: [true]
    });
  }


  save(): void {
    if (this.form.invalid) {
      this.commonService.showSnackbar('Please complete all required fields', 'ERROR', 3000);
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.commonService.loaderState(true);

    if (this.mode === 'create') {
      const payload = this.form.getRawValue() as CreateAdminUserRequest;
      this.adminDataService.createUser(payload)
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('User created successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to create user', 'ERROR', 3000);
          }
        });
    } else if (this.data.user) {
      const payload: UpdateAdminUserRequest = {
        fullName: this.form.get('fullName')?.value,
        email: this.form.get('email')?.value,
        mobileCountryCode: this.form.get('mobileCountryCode')?.value,
        mobileNumber: this.form.get('mobileNumber')?.value,
        isActive: this.form.get('isActive')?.value
      };

      this.adminDataService.updateUser(this.data.user.userID, payload)
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('User updated successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to update user', 'ERROR', 3000);
          }
        });
    }
  }

  private resetSubmitting(): void {
    this.submitting = false;
    this.commonService.loaderState(false);
  }

  get isCreate(): boolean {
    return this.mode === 'create';
  }

  close(): void {
    if (!this.submitting) {
      this.dialogRef.close(false);
    }
  }
}
