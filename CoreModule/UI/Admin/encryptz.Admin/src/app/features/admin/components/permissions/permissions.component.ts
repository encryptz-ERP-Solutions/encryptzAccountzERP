import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, TemplateRef, ViewChild } from '@angular/core';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AdminMenuItem, AdminModule, AdminPermission } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';

@Component({
  selector: 'app-permissions',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCardModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressBarModule,
    MatTooltipModule,
    ReactiveFormsModule
  ],
  templateUrl: './permissions.component.html',
  styleUrl: './permissions.component.scss'
})
export class PermissionsComponent implements AfterViewInit {
  permissions: AdminPermission[] = [];
  dataSource = new MatTableDataSource<AdminPermission>([]);
  modules: AdminModule[] = [];
  menuItems: AdminMenuItem[] = [];
  displayedColumns = ['permissionKey', 'description', 'module', 'menu', 'actions'];
  editingPermission: AdminPermission | null = null;
  filterControl = new FormControl('');
  loading = false;
  error?: string;

  permissionForm!: FormGroup;

  @ViewChild('permissionDialog') permissionDialog!: TemplateRef<any>;
  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private adminDataService: AdminDataService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.permissionForm = this.fb.group({
      permissionKey: ['', Validators.required],
      description: ['', Validators.required],
      moduleID: [null as number | null, Validators.required],
      menuItemID: [null as number | null]
    });
    this.dataSource.filterPredicate = this.createFilterPredicate();
  }

  getModuleName(moduleID: number): string {
    const module = this.modules.find(m => m.moduleID === moduleID);
    return module?.moduleName || '—';
  }

  getMenuText(menuItemID?: number): string {
    if (!menuItemID) return '—';
    const menu = this.menuItems.find(mi => mi.menuItemID === menuItemID);
    return menu?.menuText || '—';
  }

  getNullValue(): null {
    return null;
  }

  ngOnInit(): void {
    this.loadPermissions();
    this.loadModules();
    this.loadMenuItems();
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

  loadModules(): void {
    this.adminDataService.getModules().subscribe({
      next: modules => this.modules = modules,
      error: err => console.error('Failed to load modules:', err)
    });
  }

  loadMenuItems(): void {
    this.adminDataService.getMenuItems().subscribe({
      next: items => this.menuItems = items,
      error: err => console.error('Failed to load menu items:', err)
    });
  }

  loadPermissions(): void {
    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    
    this.adminDataService.getPermissions()
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: permissions => {
          this.permissions = permissions;
          this.dataSource.data = permissions;
          this.attachTableHelpers();
        },
        error: err => {
          this.error = err?.message || 'Unable to load permissions';
          this.commonService.showSnackbar(err?.message || 'Unable to load permissions', 'ERROR', 3000);
          this.permissions = [];
          this.dataSource.data = [];
        }
      });
  }

  openDialog(permission?: AdminPermission): void {
    this.editingPermission = permission ?? null;
    if (permission) {
      this.permissionForm.patchValue({
        permissionKey: permission.permissionKey,
        description: permission.description,
        moduleID: permission.moduleID,
        menuItemID: permission.menuItemID ?? null
      });
    } else {
      this.permissionForm.reset({
        permissionKey: '',
        description: '',
        moduleID: null,
        menuItemID: null
      });
    }

    this.dialog.open(this.permissionDialog, { width: '520px' });
  }

  savePermission(): void {
    if (this.permissionForm.invalid) {
      this.commonService.showSnackbar('Please complete required fields', 'ERROR', 2500);
      return;
    }

    const payload = this.permissionForm.getRawValue() as AdminPermission;

    if (this.editingPermission) {
      const updated: AdminPermission = {
        ...this.editingPermission,
        ...payload,
        menuItemID: payload.menuItemID || undefined
      };
      this.adminDataService.updatePermission(updated).subscribe({
        next: () => {
          this.commonService.showSnackbar('Permission updated', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadPermissions();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to update permission', 'ERROR', 3000)
      });
    } else {
      const createPayload = {
        permissionKey: payload.permissionKey || undefined,
        description: payload.description || undefined,
        moduleID: payload.moduleID || undefined,
        menuItemID: payload.menuItemID || undefined
      };
      this.adminDataService.createPermission(createPayload).subscribe({
        next: () => {
          this.commonService.showSnackbar('Permission created', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadPermissions();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to create permission', 'ERROR', 3000)
      });
    }
  }

  deletePermission(permission: AdminPermission): void {
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete permission',
        description: `Are you sure you want to remove "${permission.permissionKey}"? This action cannot be undone.`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminDataService.deletePermission(permission.permissionID)
          .pipe(finalize(() => {
            // Operation complete
          }))
          .subscribe({
            next: () => {
              this.commonService.showSnackbar('Permission deleted successfully', 'SUCCESS', 2500);
              this.loadPermissions();
            },
            error: err => {
              this.commonService.showSnackbar(err?.message || 'Unable to delete permission', 'ERROR', 3000);
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
    this.loadPermissions();
  }

  get hasPermissions(): boolean {
    return this.dataSource.filteredData.length > 0;
  }

  private attachTableHelpers(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
      this.dataSource.sort.active = 'permissionKey';
      this.dataSource.sort.direction = 'asc';
    }
  }

  private createFilterPredicate() {
    return (data: AdminPermission, filterValue: string): boolean => {
      if (!filterValue) {
        return true;
      }

      const term = filterValue.trim().toLowerCase();
      const moduleName = this.getModuleName(data.moduleID).toLowerCase();
      const menuText = this.getMenuText(data.menuItemID).toLowerCase();
      
      const haystack = [
        data.permissionKey,
        data.description,
        moduleName,
        menuText
      ]
        .filter(Boolean)
        .map(value => value!.toLowerCase());

      return haystack.some(value => value.includes(term));
    };
  }
}

