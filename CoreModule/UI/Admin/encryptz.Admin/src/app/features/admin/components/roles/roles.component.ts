import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, TemplateRef, ViewChild } from '@angular/core';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AdminPermission, AdminRole } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';

@Component({
  selector: 'app-roles',
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
    MatCheckboxModule,
    MatCardModule,
    MatTooltipModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressBarModule,
    ReactiveFormsModule
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements AfterViewInit {
  roles: AdminRole[] = [];
  dataSource = new MatTableDataSource<AdminRole>([]);
  permissions: AdminPermission[] = [];
  rolePermissions = new Map<number, Set<number>>();
  displayedColumns = ['roleName', 'description', 'system', 'actions'];
  filterControl = new FormControl('');

  roleForm!: FormGroup;
  editingRole: AdminRole | null = null;
  selectedRole: AdminRole | null = null;
  loading = false;
  loadingPermissions = false;
  error?: string;

  @ViewChild('roleDialog') roleDialog!: TemplateRef<any>;
  @ViewChild('permissionDialog') permissionDialog!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private adminDataService: AdminDataService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.roleForm = this.fb.group({
      roleName: ['', Validators.required],
      description: [''],
      isSystemRole: [false]
    });
    this.dataSource.filterPredicate = this.createFilterPredicate();
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
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

  loadRoles(): void {
    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    this.adminDataService.getRoles()
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: roles => {
          this.roles = roles;
          this.dataSource.data = roles;
          this.attachTableHelpers();
        },
        error: err => {
          this.error = err?.message || 'Unable to load roles';
          this.commonService.showSnackbar(err?.message || 'Unable to load roles', 'ERROR', 3000);
          this.roles = [];
          this.dataSource.data = [];
        }
      });

    this.adminDataService.getRolePermissions().subscribe({
      next: list => {
        this.rolePermissions.clear();
        list.forEach(entry => {
          if (!this.rolePermissions.has(entry.roleID)) {
            this.rolePermissions.set(entry.roleID, new Set<number>());
          }
          this.rolePermissions.get(entry.roleID)!.add(entry.permissionID);
        });
      },
      error: err => {
        console.error('Failed to load role permissions:', err);
      }
    });
  }

  loadPermissions(): void {
    this.adminDataService.getPermissions().subscribe(perms => {
      this.permissions = perms;
    });
  }

  openRoleDialog(role?: AdminRole): void {
    this.editingRole = role ?? null;
    if (role) {
      this.roleForm.patchValue({
        roleName: role.roleName,
        description: role.description,
        isSystemRole: role.isSystemRole
      });
    } else {
      this.roleForm.reset({
        roleName: '',
        description: '',
        isSystemRole: false
      });
    }
    this.dialog.open(this.roleDialog);
  }

  saveRole(): void {
    if (this.roleForm.invalid) {
      this.commonService.showSnackbar('Role name is required', 'ERROR', 2500);
      return;
    }

    const payload = this.roleForm.getRawValue();
    if (this.editingRole) {
      const updated: AdminRole = {
        ...this.editingRole,
        roleName: payload.roleName!,
        description: payload.description ?? '',
        isSystemRole: payload.isSystemRole ?? false
      };
      this.adminDataService.updateRole(updated).subscribe({
        next: () => {
          this.commonService.showSnackbar('Role updated', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadRoles();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to update role', 'ERROR', 3000)
      });
    } else {
      this.adminDataService.createRole(payload as Partial<AdminRole>).subscribe({
        next: () => {
          this.commonService.showSnackbar('Role created', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadRoles();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to create role', 'ERROR', 3000)
      });
    }
  }

  deleteRole(role: AdminRole): void {
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete role',
        description: `Are you sure you want to remove "${role.roleName}"? This action cannot be undone.`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminDataService.deleteRole(role.roleID).subscribe({
          next: () => {
            this.commonService.showSnackbar('Role deleted successfully', 'SUCCESS', 2500);
            this.loadRoles();
          },
          error: err => this.commonService.showSnackbar(err?.message || 'Unable to delete role', 'ERROR', 3000)
        });
      }
    });
  }

  getPermissionSet(roleId: number): Set<number> {
    if (!this.rolePermissions.has(roleId)) {
      this.rolePermissions.set(roleId, new Set<number>());
    }
    return this.rolePermissions.get(roleId)!;
  }

  openPermissionDialog(role: AdminRole): void {
    this.selectedRole = role;
    this.dialog.open(this.permissionDialog, { width: '520px' });
  }

  togglePermission(permission: AdminPermission, checked: boolean): void {
    if (!this.selectedRole) {
      return;
    }
    const roleId = this.selectedRole.roleID;
    if (checked) {
      this.adminDataService.addPermissionToRole(roleId, permission.permissionID).subscribe({
        next: () => {
          this.getPermissionSet(roleId).add(permission.permissionID);
          this.commonService.showSnackbar('Permission assigned', 'SUCCESS', 2000);
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to assign permission', 'ERROR', 3000)
      });
    } else {
      this.adminDataService.removePermissionFromRole(roleId, permission.permissionID).subscribe({
        next: () => {
          this.getPermissionSet(roleId).delete(permission.permissionID);
          this.commonService.showSnackbar('Permission revoked', 'SUCCESS', 2000);
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to revoke permission', 'ERROR', 3000)
      });
    }
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  refresh(): void {
    this.loadRoles();
  }

  get hasRoles(): boolean {
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
    return (data: AdminRole, filterValue: string): boolean => {
      if (!filterValue) {
        return true;
      }

      const term = filterValue.trim().toLowerCase();
      const haystack = [
        data.roleName,
        data.description
      ]
        .filter(Boolean)
        .map(value => value!.toLowerCase());

      return haystack.some(value => value.includes(term));
    };
  }
}

