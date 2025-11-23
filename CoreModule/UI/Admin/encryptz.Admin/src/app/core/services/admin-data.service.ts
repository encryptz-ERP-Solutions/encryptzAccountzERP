import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  AdminActivity,
  AdminBusiness,
  AdminDashboardSummary,
  AdminMenuItem,
  AdminModule,
  AdminPlanUsage,
  AdminPermission,
  AdminRole,
  AdminUser,
  AuditLogEntry,
  AuditLogFilter,
  CreateAdminUserRequest,
  CreateBusinessRequest,
  MenuTreeNode,
  SubscriptionPlan,
  UpdateAdminUserRequest,
  UpdateBusinessRequest
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminDataService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getDashboardSummary(limit = 5): Observable<AdminDashboardSummary> {
    const params = new HttpParams().set('recentLimit', limit);
    return this.http.get<AdminDashboardSummary>(`${this.baseUrl}api/admin/dashboard/summary`, { params });
  }

  // Users
  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.baseUrl}api/User`).pipe(
      map(users => users?.map(user => this.normalizeUser(user)) ?? [])
    );
  }

  createUser(payload: CreateAdminUserRequest): Observable<AdminUser> {
    return this.http.post<AdminUser>(`${this.baseUrl}api/User`, payload).pipe(
      map(user => this.normalizeUser(user))
    );
  }

  updateUser(userId: string, payload: UpdateAdminUserRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/User/${userId}`, payload);
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/User/${userId}`);
  }

  // Roles
  getRoles(): Observable<AdminRole[]> {
    return this.http.get<AdminRole[]>(`${this.baseUrl}api/Roles`);
  }

  createRole(role: Partial<AdminRole>): Observable<AdminRole> {
    return this.http.post<AdminRole>(`${this.baseUrl}api/Roles`, role);
  }

  updateRole(role: AdminRole): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/Roles/${role.roleID}`, role);
  }

  deleteRole(roleId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/Roles/${roleId}`);
  }

  getRolePermissions(): Observable<{ roleID: number; permissionID: number; }[]> {
    return this.http.get<{ roleID: number; permissionID: number; }[]>(`${this.baseUrl}api/RolePermissions`);
  }

  addPermissionToRole(roleId: number, permissionId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}api/RolePermissions`, {
      roleID: roleId,
      permissionID: permissionId
    });
  }

  removePermissionFromRole(roleId: number, permissionId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/RolePermissions/${roleId}/${permissionId}`);
  }

  // Permissions
  getPermissions(): Observable<AdminPermission[]> {
    return this.http.get<AdminPermission[]>(`${this.baseUrl}api/Permissions`);
  }

  createPermission(payload: Partial<AdminPermission>): Observable<AdminPermission> {
    return this.http.post<AdminPermission>(`${this.baseUrl}api/Permissions`, payload);
  }

  updatePermission(permission: AdminPermission): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/Permissions/${permission.permissionID}`, permission);
  }

  deletePermission(permissionId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/Permissions/${permissionId}`);
  }

  // Modules
  getModules(): Observable<AdminModule[]> {
    return this.http.get<AdminModule[]>(`${this.baseUrl}api/Module`);
  }

  createModule(payload: Partial<AdminModule>): Observable<AdminModule> {
    return this.http.post<AdminModule>(`${this.baseUrl}api/Module`, payload);
  }

  updateModule(module: AdminModule): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/Module/${module.moduleID}`, module);
  }

  deleteModule(moduleId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/Module/${moduleId}`);
  }

  // Menu Items
  getMenuItems(): Observable<AdminMenuItem[]> {
    return this.http.get<AdminMenuItem[]>(`${this.baseUrl}api/MenuItem`);
  }

  getMenuItemsByModule(moduleId: number): Observable<AdminMenuItem[]> {
    return this.http.get<AdminMenuItem[]>(`${this.baseUrl}api/MenuItem/module/${moduleId}`);
  }

  createMenuItem(payload: Partial<AdminMenuItem>): Observable<AdminMenuItem> {
    return this.http.post<AdminMenuItem>(`${this.baseUrl}api/MenuItem`, payload);
  }

  updateMenuItem(menuItem: AdminMenuItem): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/MenuItem/${menuItem.menuItemID}`, menuItem);
  }

  deleteMenuItem(menuItemId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/MenuItem/${menuItemId}`);
  }

  // Subscription Plans
  getSubscriptionPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.baseUrl}api/SubscriptionPlans`);
  }

  createSubscriptionPlan(payload: Partial<SubscriptionPlan>): Observable<SubscriptionPlan> {
    return this.http.post<SubscriptionPlan>(`${this.baseUrl}api/SubscriptionPlans`, payload);
  }

  updateSubscriptionPlan(plan: SubscriptionPlan): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/SubscriptionPlans/${plan.planID}`, plan);
  }

  deleteSubscriptionPlan(planId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/SubscriptionPlans/${planId}`);
  }

  // Businesses
  getBusinesses(): Observable<AdminBusiness[]> {
    return this.http.get<AdminBusiness[]>(`${this.baseUrl}api/Business`).pipe(
      map(businesses => businesses?.map(business => this.normalizeBusiness(business)) ?? [])
    );
  }

  getBusinessById(businessId: string): Observable<AdminBusiness> {
    return this.http.get<AdminBusiness>(`${this.baseUrl}api/Business/${businessId}`).pipe(
      map(business => this.normalizeBusiness(business))
    );
  }

  createBusiness(payload: CreateBusinessRequest): Observable<AdminBusiness> {
    return this.http.post<AdminBusiness>(`${this.baseUrl}api/Business`, payload).pipe(
      map(business => this.normalizeBusiness(business))
    );
  }

  createMyBusiness(payload: CreateBusinessRequest): Observable<AdminBusiness> {
    return this.http.post<AdminBusiness>(`${this.baseUrl}api/Business/my-business`, payload).pipe(
      map(business => this.normalizeBusiness(business))
    );
  }

  updateBusiness(businessId: string, payload: UpdateBusinessRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}api/Business/${businessId}`, {
      businessID: businessId,
      ...payload
    });
  }

  deleteBusiness(businessId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}api/Business/${businessId}`);
  }

  // Audit Logs
  getAuditLogs(filter: AuditLogFilter): Observable<AuditLogEntry[]> {
    let params = new HttpParams();
    if (filter.tableName) {
      params = params.set('tableName', filter.tableName);
    }
    if (filter.action) {
      params = params.set('action', filter.action);
    }
    if (filter.startDateUtc) {
      params = params.set('startDateUtc', filter.startDateUtc);
    }
    if (filter.endDateUtc) {
      params = params.set('endDateUtc', filter.endDateUtc);
    }
    if (filter.limit) {
      params = params.set('limit', filter.limit);
    }
    return this.http.get<AuditLogEntry[]>(`${this.baseUrl}api/AuditLogs`, { params });
  }

  // Helpers for navigation
  buildMenuTree(items: AdminMenuItem[], parentId: number | undefined | null = undefined): MenuTreeNode[] {
    return items
      .filter(item => {
        // Handle both null and undefined for parentMenuItemID
        const itemParentId = item.parentMenuItemID ?? undefined;
        const searchParentId = parentId ?? undefined;
        return itemParentId === searchParentId;
      })
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(item => ({
        ...item,
        children: this.buildMenuTree(items, item.menuItemID)
      }));
  }

  private normalizeUser = (user: any): AdminUser => {
    if (!user) {
      return {
        userID: '',
        userHandle: '',
        fullName: '',
        email: '',
        mobileCountryCode: '',
        mobileNumber: '',
        isActive: false,
        createdAtUTC: ''
      };
    }

    const createdAt =
      user.createdAtUTC ??
      user.createdAtUtc ??
      user.createdAt ??
      new Date().toISOString();

    return {
      userID: user.userID ?? user.userId ?? user.id ?? '',
      userHandle: user.userHandle ?? user.userName ?? '',
      fullName: user.fullName ?? user.name ?? '',
      email: user.email ?? user.emailAddress ?? '',
      mobileCountryCode: user.mobileCountryCode ?? user.countryCode ?? '',
      mobileNumber: user.mobileNumber ?? user.phoneNumber ?? '',
      isActive: user.isActive ?? true,
      createdAtUTC: createdAt,
      updatedAtUTC: user.updatedAtUTC ?? user.updatedAtUtc ?? user.updatedAt ?? createdAt
    };
  };

  private normalizeBusiness = (business: any): AdminBusiness => {
    if (!business) {
      return {
        businessID: '',
        businessName: '',
        businessCode: '',
        isActive: false
      };
    }

    const createdAt =
      business.createdAtUTC ??
      business.createdAtUtc ??
      business.createdAt ??
      new Date().toISOString();

    return {
      businessID: business.businessID ?? business.businessId ?? business.id ?? '',
      businessName: business.businessName ?? '',
      businessCode: business.businessCode ?? '',
      addressLine1: business.addressLine1,
      addressLine2: business.addressLine2,
      city: business.city,
      stateID: business.stateID ?? business.stateId,
      pinCode: business.pinCode,
      countryID: business.countryID ?? business.countryId,
      isActive: business.isActive ?? true,
      createdByUserID: business.createdByUserID ?? business.createdByUserId,
      createdAtUTC: createdAt,
      updatedByUserID: business.updatedByUserID ?? business.updatedByUserId,
      updatedAtUTC: business.updatedAtUTC ?? business.updatedAtUtc ?? business.updatedAt ?? createdAt
    };
  };
}

