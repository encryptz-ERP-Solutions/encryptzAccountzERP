import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, TemplateRef, ViewChild } from '@angular/core';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { SubscriptionPlan } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';

@Component({
  selector: 'app-subscription-plans',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatCardModule,
    MatTooltipModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressBarModule,
    MatChipsModule,
    ReactiveFormsModule
  ],
  templateUrl: './subscription-plans.component.html',
  styleUrl: './subscription-plans.component.scss'
})
export class SubscriptionPlansComponent implements AfterViewInit {
  plans: SubscriptionPlan[] = [];
  dataSource = new MatTableDataSource<SubscriptionPlan>([]);
  displayedColumns = ['planName', 'price', 'limits', 'visibility', 'status', 'actions'];
  planForm!: FormGroup;
  editingPlan: SubscriptionPlan | null = null;
  filterControl = new FormControl('');
  loading = false;
  error?: string;

  @ViewChild('planDialog') planDialog!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private adminDataService: AdminDataService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.planForm = this.fb.group({
      planName: ['', Validators.required],
      description: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      maxUsers: [1, [Validators.required, Validators.min(1)]],
      maxBusinesses: [1, [Validators.required, Validators.min(1)]],
      isPubliclyVisible: [true],
      isActive: [true]
    });
    this.dataSource.filterPredicate = this.createFilterPredicate();
  }

  ngOnInit(): void {
    this.loadPlans();
    this.filterControl.valueChanges.subscribe(value => {
      this.applyFilter(value || '');
    });
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  loadPlans(): void {
    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    this.adminDataService.getSubscriptionPlans()
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: plans => {
          this.plans = plans;
          this.dataSource.data = plans;
          this.attachTableHelpers();
        },
        error: err => {
          this.error = err?.message || 'Unable to load subscription plans';
          this.commonService.showSnackbar(err?.message || 'Unable to load plans', 'ERROR', 3000);
          this.plans = [];
          this.dataSource.data = [];
        }
      });
  }

  openDialog(plan?: SubscriptionPlan): void {
    this.editingPlan = plan ?? null;
    if (plan) {
      this.planForm.patchValue(plan);
    } else {
      this.planForm.reset({
        planName: '',
        description: '',
        price: 0,
        maxUsers: 1,
        maxBusinesses: 1,
        isPubliclyVisible: true,
        isActive: true
      });
    }
    this.dialog.open(this.planDialog, { width: '520px' });
  }

  savePlan(): void {
    if (this.planForm.invalid) {
      this.commonService.showSnackbar('Please complete required fields', 'ERROR', 2500);
      return;
    }
    const raw = this.planForm.getRawValue();
    const payload = {
      planName: raw.planName || undefined,
      description: raw.description || undefined,
      price: raw.price ?? 0,
      maxUsers: raw.maxUsers ?? 1,
      maxBusinesses: raw.maxBusinesses ?? 1,
      isPubliclyVisible: raw.isPubliclyVisible ?? true,
      isActive: raw.isActive ?? true
    };

    if (this.editingPlan) {
      const updated: SubscriptionPlan = {
        ...this.editingPlan,
        planName: payload.planName!,
        description: payload.description!,
        price: payload.price!,
        maxUsers: payload.maxUsers!,
        maxBusinesses: payload.maxBusinesses!,
        isPubliclyVisible: payload.isPubliclyVisible!,
        isActive: payload.isActive!
      };
      this.adminDataService.updateSubscriptionPlan(updated).subscribe({
        next: () => {
          this.commonService.showSnackbar('Plan updated', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadPlans();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to update plan', 'ERROR', 3000)
      });
    } else {
      this.adminDataService.createSubscriptionPlan(payload).subscribe({
        next: () => {
          this.commonService.showSnackbar('Plan created', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadPlans();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to create plan', 'ERROR', 3000)
      });
    }
  }

  deletePlan(plan: SubscriptionPlan): void {
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete subscription plan',
        description: `Are you sure you want to remove "${plan.planName}"? This action cannot be undone.`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminDataService.deleteSubscriptionPlan(plan.planID).subscribe({
          next: () => {
            this.commonService.showSnackbar('Plan deleted successfully', 'SUCCESS', 2500);
            this.loadPlans();
          },
          error: err => this.commonService.showSnackbar(err?.message || 'Unable to delete plan', 'ERROR', 3000)
        });
      }
    });
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  refresh(): void {
    this.loadPlans();
  }

  get hasPlans(): boolean {
    return this.dataSource.filteredData.length > 0;
  }

  private attachTableHelpers(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  private createFilterPredicate() {
    return (data: SubscriptionPlan, filterValue: string): boolean => {
      if (!filterValue) {
        return true;
      }

      const term = filterValue.trim().toLowerCase();
      const haystack = [
        data.planName,
        data.description
      ]
        .filter(Boolean)
        .map(value => value!.toLowerCase());

      return haystack.some(value => value.includes(term));
    };
  }
}

