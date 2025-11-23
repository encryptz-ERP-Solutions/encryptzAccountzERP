import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from "@angular/material/card";
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AddEditBusinessComponent } from './add-edit-business/add-edit-business.component';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';
import { AdminBusiness } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-business-management',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatPaginatorModule,
    MatDialogModule,
    MatMenuModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressBarModule,
    MatSortModule,
    ReactiveFormsModule
  ],
  templateUrl: './business-management.component.html',
  styleUrl: './business-management.component.scss'
})
export class BusinessManagementComponent implements AfterViewInit {

  displayedColumns = ['businessName', 'businessCode', 'location', 'created', 'status', 'actions'];
  dataSource = new MatTableDataSource<AdminBusiness>([]);
  filterControl = new FormControl('');
  loading = false;
  error?: string;
  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private dialog: MatDialog,
    private adminDataService: AdminDataService,
    private commonService: CommonService
  ) {
    this.dataSource.filterPredicate = this.createFilterPredicate();
  }

  ngOnInit() {
    this.loadBusinesses();
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

  loadBusinesses(): void {
    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    this.adminDataService.getBusinesses()
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: businesses => {
          this.dataSource.data = businesses;
          this.attachTableHelpers();
        },
        error: err => {
          this.error = err?.message || 'Unable to load businesses';
          this.commonService.showSnackbar(err?.message || 'Unable to load businesses', 'ERROR', 3000);
          this.dataSource.data = [];
        }
      });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(AddEditBusinessComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadBusinesses();
      }
    });
  }

  openEditDialog(business: AdminBusiness): void {
    const dialogRef = this.dialog.open(AddEditBusinessComponent, {
      width: '600px',
      data: { mode: 'edit', business }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadBusinesses();
      }
    });
  }

  deleteBusiness(business: AdminBusiness): void {
    const businessId = business.businessID;
    if (!businessId) {
      this.commonService.showSnackbar('Unable to resolve business identifier', 'ERROR', 3000);
      return;
    }
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete business',
        description: `Are you sure you want to remove "${business.businessName}"?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminDataService.deleteBusiness(businessId).subscribe({
          next: () => {
            this.commonService.showSnackbar('Business deleted successfully', 'SUCCESS', 3000);
            this.loadBusinesses();
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to delete business', 'ERROR', 3000);
          }
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
    this.loadBusinesses();
  }

  get hasBusinesses(): boolean {
    return this.dataSource.filteredData.length > 0;
  }

  private attachTableHelpers(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
      // Set custom sort accessor for 'created' column
      this.dataSource.sortingDataAccessor = (item: AdminBusiness, property: string) => {
        switch (property) {
          case 'created':
            return item.createdAtUTC ? new Date(item.createdAtUTC).getTime() : 0;
          default:
            return (item as any)[property];
        }
      };
      this.dataSource.sort.active = 'created';
      this.dataSource.sort.direction = 'desc';
    }
  }

  private createFilterPredicate() {
    return (data: AdminBusiness, filterValue: string): boolean => {
      if (!filterValue) {
        return true;
      }

      const term = filterValue.trim().toLowerCase();
      const haystack = [
        data.businessName,
        data.businessCode,
        data.city,
        data.addressLine1,
        data.addressLine2,
        data.pinCode
      ]
        .filter(Boolean)
        .map(value => value!.toLowerCase());

      return haystack.some(value => value.includes(term));
    };
  }
}

