import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { ChartOfAccountsService, ChartOfAccount, AccountType, CreateChartOfAccountRequest, UpdateChartOfAccountRequest } from '../../../../../../core/services/chart-of-accounts.service';
import { CommonService } from '../../../../../../shared/services/common.service';
import { BusinessContextService } from '../../../../../../core/services/business-context.service';

@Component({
  selector: 'app-add-edit-chart-of-account',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    ReactiveFormsModule
  ],
  templateUrl: './add-edit-chart-of-account.component.html',
  styleUrl: './add-edit-chart-of-account.component.scss'
})
export class AddEditChartOfAccountComponent implements OnInit {
  form!: FormGroup;
  mode: 'create' | 'edit' = 'create';
  submitting = false;
  accountTypes: AccountType[] = [];
  parentAccounts: ChartOfAccount[] = [];
  loadingAccountTypes = false;
  loadingParentAccounts = false;

  constructor(
    private dialogRef: MatDialogRef<AddEditChartOfAccountComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit', account?: ChartOfAccount, parentAccounts?: ChartOfAccount[] },
    private fb: FormBuilder,
    private chartOfAccountsService: ChartOfAccountsService,
    private commonService: CommonService,
    private businessContext: BusinessContextService
  ) {
    this.dialogRef.disableClose = true;
  }

  ngOnInit() {
    this.mode = this.data?.mode ?? 'create';
    this.buildForm();
    this.loadAccountTypes();
    
    if (this.data?.parentAccounts) {
      this.parentAccounts = this.data.parentAccounts;
    } else {
      this.loadParentAccounts();
    }

    if (this.mode === 'edit' && this.data.account) {
      this.form.patchValue({
        accountCode: this.data.account.accountCode,
        accountName: this.data.account.accountName,
        accountTypeID: this.data.account.accountTypeID,
        parentAccountID: this.data.account.parentAccountID,
        description: this.data.account.description,
        isActive: this.data.account.isActive
      });
      // Account code cannot be changed in edit mode
      this.form.get('accountCode')?.disable();
      this.form.get('accountTypeID')?.disable();
    }
  }

  private buildForm(): void {
    this.form = this.fb.group({
      accountCode: ['', [Validators.required, Validators.maxLength(20)]],
      accountName: ['', [Validators.required, Validators.maxLength(200)]],
      accountTypeID: ['', [Validators.required]],
      parentAccountID: [null],
      description: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  private loadAccountTypes(): void {
    this.loadingAccountTypes = true;
    this.chartOfAccountsService.getAccountTypes()
      .pipe(finalize(() => this.loadingAccountTypes = false))
      .subscribe({
        next: types => {
          this.accountTypes = types;
        },
        error: err => {
          this.commonService.showSnackbar('Unable to load account types', 'ERROR', 3000);
        }
      });
  }

  private loadParentAccounts(): void {
    const business = this.businessContext.currentBusiness;
    if (!business) {
      return;
    }

    this.loadingParentAccounts = true;
    this.chartOfAccountsService.getAll(business.businessID)
      .pipe(finalize(() => this.loadingParentAccounts = false))
      .subscribe({
        next: accounts => {
          // Filter out system accounts and the current account (if editing)
          this.parentAccounts = accounts.filter(acc => 
            !acc.isSystemAccount && 
            (this.mode === 'create' || acc.accountID !== this.data.account?.accountID)
          );
        },
        error: err => {
          this.commonService.showSnackbar('Unable to load parent accounts', 'ERROR', 3000);
        }
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.commonService.showSnackbar('Please complete all required fields', 'ERROR', 3000);
      this.form.markAllAsTouched();
      return;
    }

    const business = this.businessContext.currentBusiness;
    if (!business) {
      this.commonService.showSnackbar('No business selected', 'ERROR', 3000);
      return;
    }

    this.submitting = true;
    this.commonService.loaderState(true);

    if (this.mode === 'create') {
      const formValue = this.form.getRawValue();
      const payload: CreateChartOfAccountRequest = {
        businessID: business.businessID,
        accountTypeID: formValue.accountTypeID,
        parentAccountID: formValue.parentAccountID || null,
        accountCode: formValue.accountCode,
        accountName: formValue.accountName,
        description: formValue.description || null
      };

      this.chartOfAccountsService.create(payload)
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('Account created successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to create account', 'ERROR', 3000);
          }
        });
    } else if (this.data.account) {
      const formValue = this.form.getRawValue();
      const payload: UpdateChartOfAccountRequest = {
        accountName: formValue.accountName,
        description: formValue.description || null,
        isActive: formValue.isActive
      };

      this.chartOfAccountsService.update(this.data.account.accountID, payload)
        .pipe(finalize(() => this.resetSubmitting()))
        .subscribe({
          next: () => {
            this.commonService.showSnackbar('Account updated successfully', 'SUCCESS', 3000);
            this.dialogRef.close(true);
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to update account', 'ERROR', 3000);
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

