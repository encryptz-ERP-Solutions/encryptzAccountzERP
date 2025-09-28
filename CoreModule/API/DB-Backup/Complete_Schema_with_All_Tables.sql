-- This script creates all necessary tables for the refactored API and new features.
-- Please execute this script against your database to ensure full functionality.

-- Create schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
BEGIN
    EXEC('CREATE SCHEMA core');
END
GO

-- 1. Users Table
PRINT 'Creating table core.Users...';
CREATE TABLE core.Users (
    UserID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserHandle NVARCHAR(50) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    HashedPassword NVARCHAR(255),
    MobileCountryCode NVARCHAR(10),
    MobileNumber NVARCHAR(20),
    PanCardNumber_Encrypted VARBINARY(MAX),
    AadharNumber_Encrypted VARBINARY(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO
PRINT 'Table core.Users created successfully.';

-- 2. Businesses Table
PRINT 'Creating table core.Businesses...';
CREATE TABLE core.Businesses (
    BusinessID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessName NVARCHAR(100) NOT NULL,
    BusinessCode NVARCHAR(20) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    Gstin NVARCHAR(15),
    TanNumber NVARCHAR(10),
    AddressLine1 NVARCHAR(200),
    AddressLine2 NVARCHAR(200),
    City NVARCHAR(50),
    StateID INT,
    PinCode NVARCHAR(10),
    CountryID INT,
    CreatedByUserID UNIQUEIDENTIFIER NOT NULL,
    CreatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedByUserID UNIQUEIDENTIFIER NOT NULL,
    UpdatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Businesses_CreatedByUser FOREIGN KEY (CreatedByUserID) REFERENCES core.Users(UserID),
    CONSTRAINT FK_Businesses_UpdatedByUser FOREIGN KEY (UpdatedByUserID) REFERENCES core.Users(UserID)
);
GO
PRINT 'Table core.Businesses created successfully.';

-- 3. Modules Table
PRINT 'Creating table core.Modules...';
CREATE TABLE core.Modules (
    ModuleID INT IDENTITY(1,1) PRIMARY KEY,
    ModuleName NVARCHAR(100) NOT NULL,
    IsSystemModule BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1
);
GO
PRINT 'Table core.Modules created successfully.';

-- 4. MenuItems Table
PRINT 'Creating table core.MenuItems...';
CREATE TABLE core.MenuItems (
    MenuItemID INT IDENTITY(1,1) PRIMARY KEY,
    ModuleID INT NOT NULL,
    ParentMenuItemID INT,
    MenuText NVARCHAR(100) NOT NULL,
    MenuURL NVARCHAR(200),
    IconClass NVARCHAR(50),
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_MenuItems_Module FOREIGN KEY (ModuleID) REFERENCES core.Modules(ModuleID),
    CONSTRAINT FK_MenuItems_ParentMenuItem FOREIGN KEY (ParentMenuItemID) REFERENCES core.MenuItems(MenuItemID)
);
GO
PRINT 'Table core.MenuItems created successfully.';

-- 5. Permissions Table
PRINT 'Creating table core.Permissions...';
CREATE TABLE core.Permissions (
    PermissionID INT IDENTITY(1,1) PRIMARY KEY,
    PermissionKey NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NOT NULL,
    ModuleID INT NOT NULL,
    MenuItemID INT,
    CONSTRAINT FK_Permissions_Module FOREIGN KEY (ModuleID) REFERENCES core.Modules(ModuleID),
    CONSTRAINT FK_Permissions_MenuItem FOREIGN KEY (MenuItemID) REFERENCES core.MenuItems(MenuItemID)
);
GO
PRINT 'Table core.Permissions created successfully.';

-- 6. Roles Table
PRINT 'Creating table core.Roles...';
CREATE TABLE core.Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsSystemRole BIT NOT NULL DEFAULT 0
);
GO
PRINT 'Table core.Roles created successfully.';

-- 7. RolePermissions Junction Table
PRINT 'Creating table core.RolePermissions...';
CREATE TABLE core.RolePermissions (
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (RoleID, PermissionID),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleID) REFERENCES core.Roles(RoleID) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionID) REFERENCES core.Permissions(PermissionID) ON DELETE CASCADE
);
GO
PRINT 'Table core.RolePermissions created successfully.';

-- 8. UserBusinessRoles Junction Table
PRINT 'Creating table core.UserBusinessRoles...';
CREATE TABLE core.UserBusinessRoles (
    UserID UNIQUEIDENTIFIER NOT NULL,
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, BusinessID, RoleID),
    CONSTRAINT FK_UserBusinessRoles_User FOREIGN KEY (UserID) REFERENCES core.Users(UserID),
    CONSTRAINT FK_UserBusinessRoles_Business FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID),
    CONSTRAINT FK_UserBusinessRoles_Role FOREIGN KEY (RoleID) REFERENCES core.Roles(RoleID)
);
GO
PRINT 'Table core.UserBusinessRoles created successfully.';

-- 9. SubscriptionPlans Table
PRINT 'Creating table core.SubscriptionPlans...';
CREATE TABLE core.SubscriptionPlans (
    PlanID INT IDENTITY(1,1) PRIMARY KEY,
    PlanName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    MaxUsers INT NOT NULL,
    MaxBusinesses INT NOT NULL,
    IsPubliclyVisible BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1
);
GO
PRINT 'Table core.SubscriptionPlans created successfully.';

-- 10. SubscriptionPlanPermissions Junction Table
PRINT 'Creating table core.SubscriptionPlanPermissions...';
CREATE TABLE core.SubscriptionPlanPermissions (
    PlanID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (PlanID, PermissionID),
    CONSTRAINT FK_SubscriptionPlanPermissions_Plan FOREIGN KEY (PlanID) REFERENCES core.SubscriptionPlans(PlanID) ON DELETE CASCADE,
    CONSTRAINT FK_SubscriptionPlanPermissions_Permission FOREIGN KEY (PermissionID) REFERENCES core.Permissions(PermissionID) ON DELETE CASCADE
);
GO
PRINT 'Table core.SubscriptionPlanPermissions created successfully.';

-- 11. UserSubscriptions Table
PRINT 'Creating table core.UserSubscriptions...';
CREATE TABLE core.UserSubscriptions (
    SubscriptionID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    PlanID INT NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- e.g., 'Active', 'Canceled', 'PastDue'
    StartDateUTC DATETIME2 NOT NULL,
    EndDateUTC DATETIME2 NOT NULL,
    TrialEndsAtUTC DATETIME2,
    CreatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserSubscriptions_Business FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID),
    CONSTRAINT FK_UserSubscriptions_Plan FOREIGN KEY (PlanID) REFERENCES core.SubscriptionPlans(PlanID)
);
GO
PRINT 'Table core.UserSubscriptions created successfully.';

-- 12. OneTimePasswords Table
PRINT 'Creating table core.OneTimePasswords...';
CREATE TABLE core.OneTimePasswords (
    OtpID BIGINT IDENTITY(1,1) PRIMARY KEY,
    LoginIdentifier NVARCHAR(100) NOT NULL,
    OTP NVARCHAR(6) NOT NULL,
    ExpiryTimeUTC DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE INDEX IX_OneTimePasswords_LoginIdentifier ON core.OneTimePasswords(LoginIdentifier);
GO
PRINT 'Table core.OneTimePasswords created successfully.';

PRINT 'All tables created successfully. Database schema setup is complete.';