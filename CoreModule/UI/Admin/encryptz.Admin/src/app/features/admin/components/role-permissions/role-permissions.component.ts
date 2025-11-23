import { CommonModule } from '@angular/common';
import { AfterViewInit, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { CommonService } from '../../../../shared/services/common.service';
import { AdminRole, AdminPermission, AdminModule } from '../../../../core/models/admin.models';

interface PermissionGroup {
  moduleID: number;
  moduleName: string;
  permissions: AdminPermission[];
}

@Component({
  selector: 'app-role-permissions',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule,
    ReactiveFormsModule
  ],
  templateUrl: './role-permissions.component.html',
  styleUrl: './role-permissions.component.scss'
})
export class RolePermissionsComponent implements AfterViewInit {
  roles: AdminRole[] = [];
  permissions: AdminPermission[] = [];
  permissionGroups: PermissionGroup[] = [];
  selectedRole: AdminRole | null = null;
  rolePermissions = new Map<number, Set<number>>();
  
  roleControl = new FormControl<number | null>(null);
  searchControl = new FormControl('');
  moduleFilterControl = new FormControl<number | null>(null);
  
  loading = false;
  loadingPermissions = false;
  saving = false;
  error?: string;
  
  filteredPermissions: AdminPermission[] = [];
  modules: AdminModule[] = [];

  constructor(
    private adminDataService: AdminDataService,
    private commonService: CommonService
  ) {
    this.roleControl.valueChanges.subscribe(roleId => {
      if (roleId) {
        const role = this.roles.find(r => r.roleID === roleId);
        this.selectedRole = role || null;
        this.loadRolePermissions(roleId);
      } else {
        this.selectedRole = null;
      }
    });

    this.searchControl.valueChanges.subscribe(() => {
      this.applyFilters();
    });

    this.moduleFilterControl.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
    this.loadModules();
  }

  ngAfterViewInit(): void {
    // Component initialization complete
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
          if (roles.length > 0 && !this.selectedRole) {
            this.roleControl.setValue(roles[0].roleID);
          }
        },
        error: err => {
          this.error = err?.message || 'Unable to load roles';
          this.commonService.showSnackbar(err?.message || 'Unable to load roles', 'ERROR', 3000);
          this.roles = [];
        }
      });
  }

  loadPermissions(): void {
    this.loadingPermissions = true;
    
    this.adminDataService.getPermissions()
      .pipe(finalize(() => {
        this.loadingPermissions = false;
      }))
      .subscribe({
        next: permissions => {
          this.permissions = permissions;
          this.groupPermissionsByModule();
          this.applyFilters();
        },
        error: err => {
          this.commonService.showSnackbar(err?.message || 'Unable to load permissions', 'ERROR', 3000);
          this.permissions = [];
        }
      });
  }

  loadRolePermissions(roleId: number): void {
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

  groupPermissionsByModule(): void {
    const groupsMap = new Map<number, PermissionGroup>();
    
    // Use filtered permissions if available, otherwise use all permissions
    const permsToGroup = this.filteredPermissions.length > 0 
      ? this.filteredPermissions 
      : this.permissions;
    
    permsToGroup.forEach(permission => {
      if (!groupsMap.has(permission.moduleID)) {
        groupsMap.set(permission.moduleID, {
          moduleID: permission.moduleID,
          moduleName: this.getModuleName(permission.moduleID),
          permissions: []
        });
      }
      groupsMap.get(permission.moduleID)!.permissions.push(permission);
    });

    this.permissionGroups = Array.from(groupsMap.values()).sort((a, b) => 
      a.moduleName.localeCompare(b.moduleName)
    );
  }

  loadModules(): void {
    this.adminDataService.getModules().subscribe({
      next: modules => {
        this.modules = modules;
        this.groupPermissionsByModule();
      },
      error: err => {
        console.error('Failed to load modules:', err);
      }
    });
  }

  getModuleName(moduleID: number): string {
    const module = this.modules.find(m => m.moduleID === moduleID);
    return module?.moduleName || `Module ${moduleID}`;
  }

  applyFilters(): void {
    let filtered = [...this.permissions];
    
    // Apply search filter
    const searchTerm = this.searchControl.value?.toLowerCase() || '';
    if (searchTerm) {
      filtered = filtered.filter(p => 
        p.permissionKey.toLowerCase().includes(searchTerm) ||
        p.description.toLowerCase().includes(searchTerm)
      );
    }

    // Apply module filter
    const moduleId = this.moduleFilterControl.value;
    if (moduleId) {
      filtered = filtered.filter(p => p.moduleID === moduleId);
    }

    this.filteredPermissions = filtered;
    this.groupPermissionsByModule();
  }

  hasPermission(roleId: number, permissionId: number): boolean {
    const permissionSet = this.rolePermissions.get(roleId);
    return permissionSet ? permissionSet.has(permissionId) : false;
  }

  togglePermission(permission: AdminPermission, checked: boolean): void {
    if (!this.selectedRole) {
      return;
    }

    const roleId = this.selectedRole.roleID;
    this.saving = true;

    const operation = checked
      ? this.adminDataService.addPermissionToRole(roleId, permission.permissionID)
      : this.adminDataService.removePermissionFromRole(roleId, permission.permissionID);

    operation
      .pipe(finalize(() => {
        this.saving = false;
      }))
      .subscribe({
        next: () => {
          if (!this.rolePermissions.has(roleId)) {
            this.rolePermissions.set(roleId, new Set<number>());
          }
          
          if (checked) {
            this.rolePermissions.get(roleId)!.add(permission.permissionID);
            this.commonService.showSnackbar('Permission assigned', 'SUCCESS', 2000);
          } else {
            this.rolePermissions.get(roleId)!.delete(permission.permissionID);
            this.commonService.showSnackbar('Permission revoked', 'SUCCESS', 2000);
          }
        },
        error: err => {
          this.commonService.showSnackbar(
            err?.message || (checked ? 'Unable to assign permission' : 'Unable to revoke permission'),
            'ERROR',
            3000
          );
        }
      });
  }

  selectAllInModule(moduleID: number): void {
    if (!this.selectedRole) {
      return;
    }

    const modulePermissions = this.filteredPermissions.filter(p => p.moduleID === moduleID);
    const roleId = this.selectedRole.roleID;
    const permissionSet = this.rolePermissions.get(roleId) || new Set<number>();
    
    const allSelected = modulePermissions.every(p => permissionSet.has(p.permissionID));
    
    modulePermissions.forEach(permission => {
      const hasPermission = permissionSet.has(permission.permissionID);
      if (allSelected && hasPermission) {
        this.togglePermission(permission, false);
      } else if (!allSelected && !hasPermission) {
        this.togglePermission(permission, true);
      }
    });
  }

  isModuleFullySelected(moduleID: number): boolean {
    if (!this.selectedRole) {
      return false;
    }

    const modulePermissions = this.filteredPermissions.filter(p => p.moduleID === moduleID);
    if (modulePermissions.length === 0) {
      return false;
    }

    const permissionSet = this.rolePermissions.get(this.selectedRole.roleID) || new Set<number>();
    return modulePermissions.every(p => permissionSet.has(p.permissionID));
  }

  getSelectedCount(roleId: number): number {
    const permissionSet = this.rolePermissions.get(roleId);
    return permissionSet ? permissionSet.size : 0;
  }

  refresh(): void {
    if (this.selectedRole) {
      this.loadRolePermissions(this.selectedRole.roleID);
    }
    this.loadPermissions();
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.moduleFilterControl.setValue(null);
  }
}

