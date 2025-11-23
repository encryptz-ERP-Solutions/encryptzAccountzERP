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
import { AddEditUserComponent } from './add-edit-user/add-edit-user.component';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';
import { AdminUser } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-user-management',
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
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements AfterViewInit {

  displayedColumns = ['userHandle', 'fullName', 'email', 'phone', 'created', 'status', 'actions'];
  dataSource = new MatTableDataSource<AdminUser>([]);
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
    this.loadUsers();
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

  loadUsers(): void {
    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    this.adminDataService.getUsers()
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: users => {
          this.dataSource.data = users;
          this.attachTableHelpers();
        },
        error: err => {
          this.error = err?.message || 'Unable to load users';
          this.commonService.showSnackbar(err?.message || 'Unable to load users', 'ERROR', 3000);
          this.dataSource.data = [];
        }
      });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      width: '520px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadUsers();
      }
    });
  }

  openEditDialog(user: AdminUser): void {
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      width: '520px',
      data: { mode: 'edit', user }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadUsers();
      }
    });
  }

  deleteUser(user: AdminUser): void {
    const userId = user.userID ?? (user as any).userId;
    if (!userId) {
      this.commonService.showSnackbar('Unable to resolve user identifier', 'ERROR', 3000);
      return;
    }
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete user',
        description: `Are you sure you want to remove "${user.fullName}"?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminDataService.deleteUser(userId).subscribe({
          next: () => {
            this.commonService.showSnackbar('User deleted successfully', 'SUCCESS', 3000);
            this.loadUsers();
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to delete user', 'ERROR', 3000);
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
    this.loadUsers();
  }

  get hasUsers(): boolean {
    return this.dataSource.filteredData.length > 0;
  }

  private attachTableHelpers(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
      this.dataSource.sort.active = 'created';
      this.dataSource.sort.direction = 'desc';
    }
  }

  private createFilterPredicate() {
    return (data: AdminUser, filterValue: string): boolean => {
      if (!filterValue) {
        return true;
      }

      const term = filterValue.trim().toLowerCase();
      const haystack = [
        data.userHandle,
        data.fullName,
        data.email,
        `${data.mobileCountryCode ?? ''}${data.mobileNumber ?? ''}`
      ]
        .filter(Boolean)
        .map(value => value!.toLowerCase());

      return haystack.some(value => value.includes(term));
    };
  }
}
