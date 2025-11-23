export interface AdminDashboardSummary {
  kpis: AdminDashboardKpi;
  recentUsers: AdminUserSummary[];
  recentBusinesses: AdminBusinessSummary[];
  planUsage: AdminPlanUsage[];
  recentActivity: AdminActivity[];
}

export interface AdminDashboardKpi {
  totalUsers: number;
  activeUsers: number;
  totalBusinesses: number;
  activeBusinesses: number;
  totalRoles: number;
  totalPermissions: number;
  totalModules: number;
  totalMenuItems: number;
  subscriptionPlans: number;
}

export interface AdminUserSummary {
  userId: string;
  userHandle: string;
  fullName: string;
  email?: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface AdminBusinessSummary {
  businessId: string;
  businessName: string;
  city?: string;
  state?: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface AdminPlanUsage {
  planId: number;
  planName: string;
  price: number;
  isActive: boolean;
  activeSubscriptions: number;
}

export interface AdminActivity {
  auditLogId: number;
  tableName: string;
  recordId: string;
  action: string;
  changedByUserName?: string;
  changedAtUtc: string;
  description?: string;
}

export interface AdminModule {
  moduleID: number;
  moduleName: string;
  isSystemModule: boolean;
  isActive: boolean;
}

export interface AdminMenuItem {
  menuItemID: number;
  moduleID: number;
  parentMenuItemID?: number;
  menuText: string;
  menuURL?: string;
  iconClass?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface AdminRole {
  roleID: number;
  roleName: string;
  description?: string;
  isSystemRole: boolean;
}

export interface AdminPermission {
  permissionID: number;
  permissionKey: string;
  description: string;
  menuItemID?: number;
  moduleID: number;
}

export interface AdminUser {
  userID: string;
  userHandle: string;
  fullName: string;
  email?: string;
  mobileCountryCode?: string;
  mobileNumber?: string;
  isActive: boolean;
  createdAtUTC: string;
  updatedAtUTC?: string;
}

export interface CreateAdminUserRequest {
  userHandle: string;
  fullName: string;
  email: string;
  password: string;
  mobileCountryCode?: string;
  mobileNumber?: string;
}

export interface UpdateAdminUserRequest {
  fullName?: string;
  email?: string;
  mobileCountryCode?: string;
  mobileNumber?: string;
  isActive?: boolean;
}

export interface SubscriptionPlan {
  planID: number;
  planName: string;
  description: string;
  price: number;
  maxUsers: number;
  maxBusinesses: number;
  isPubliclyVisible: boolean;
  isActive: boolean;
}

export interface AuditLogEntry {
  auditLogId: number;
  tableName: string;
  recordId: string;
  action: string;
  changedByUserName?: string;
  changedAtUtc: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface AuditLogFilter {
  tableName?: string;
  action?: string;
  startDateUtc?: string;
  endDateUtc?: string;
  limit?: number;
}

export interface MenuTreeNode extends AdminMenuItem {
  children: MenuTreeNode[];
}

export interface AdminBusiness {
  businessID: string;
  businessName: string;
  businessCode: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateID?: number;
  pinCode?: string;
  countryID?: number;
  isActive: boolean;
  createdByUserID?: string;
  createdAtUTC?: string;
  updatedByUserID?: string;
  updatedAtUTC?: string;
}

export interface CreateBusinessRequest {
  businessName: string;
  businessCode?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateID?: number;
  pinCode?: string;
  countryID?: number;
  isActive?: boolean;
}

export interface UpdateBusinessRequest {
  businessName?: string;
  businessCode?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateID?: number;
  pinCode?: string;
  countryID?: number;
  isActive?: boolean;
}

