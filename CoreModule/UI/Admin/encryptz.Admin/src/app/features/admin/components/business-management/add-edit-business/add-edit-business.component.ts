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
import { AdminBusiness, CreateBusinessRequest, UpdateBusinessRequest } from '../../../../../core/models/admin.models';

@Component({
  selector: 'app-add-edit-business',
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
  templateUrl: './add-edit-business.component.html',
  styleUrl: './add-edit-business.component.scss'
})
export class AddEditBusinessComponent {
  form!: FormGroup;
  mode: 'create' | 'edit' = 'create';
  submitting = false;

  constructor(
    private dialogRef: MatDialogRef<AddEditBusinessComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit', business?: AdminBusiness, useMyBusinessEndpoint?: boolean },
    private fb: FormBuilder,
    private adminDataService: AdminDataService,
    private commonService: CommonService,
  ) {
    this.dialogRef.disableClose = true;
  }

  ngOnInit() {
    this.mode = this.data?.mode ?? 'create';
    this.buildForm();

    if (this.mode === 'edit' && this.data.business) {
      this.form.patchValue({
        businessName: this.data.business.businessName,
        businessCode: this.data.business.businessCode,
        addressLine1: this.data.business.addressLine1,
        addressLine2: this.data.business.addressLine2,
        city: this.data.business.city,
        stateID: this.data.business.stateID,
        pinCode: this.data.business.pinCode,
        countryID: this.data.business.countryID,
        isActive: this.data.business.isActive
      });
    }
  }

  private buildForm(): void {
    this.form = this.fb.group({
      businessName: ['', [Validators.required]],
      businessCode: [''],
      addressLine1: [''],
      addressLine2: [''],
      city: [''],
      stateID: [null],
      pinCode: [''],
      countryID: [null],
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
      const payload = this.form.getRawValue() as CreateBusinessRequest;
      // Use my-business endpoint if called from dashboard, otherwise use admin endpoint
      const createMethod = this.data?.useMyBusinessEndpoint 
        ? this.adminDataService.createMyBusiness(payload)
        : this.adminDataService.createBusiness(payload);
      
      createMethod
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('Business created successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to create business', 'ERROR', 3000);
          }
        });
    } else if (this.data.business) {
      const payload: UpdateBusinessRequest = {
        businessName: this.form.get('businessName')?.value,
        businessCode: this.form.get('businessCode')?.value,
        addressLine1: this.form.get('addressLine1')?.value,
        addressLine2: this.form.get('addressLine2')?.value,
        city: this.form.get('city')?.value,
        stateID: this.form.get('stateID')?.value,
        pinCode: this.form.get('pinCode')?.value,
        countryID: this.form.get('countryID')?.value,
        isActive: this.form.get('isActive')?.value
      };

      this.adminDataService.updateBusiness(this.data.business.businessID, payload)
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('Business updated successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to update business', 'ERROR', 3000);
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

