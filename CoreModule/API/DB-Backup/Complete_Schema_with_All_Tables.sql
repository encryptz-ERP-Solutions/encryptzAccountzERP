/*
================================================================================================
Filename: encryptzERPCore_Schema.sql
Description: Database schema for the Core Module of the Encryptz ERP.
Version: 4.0
Change: Corrected table definitions to align perfectly with the seeding script.
================================================================================================
*/

-- Use your target database
USE encryptzERPCore;
GO

-- Drop existing tables in reverse order of dependency to avoid errors if re-running script
IF OBJECT_ID('core.UserBusinessRoles', 'U') IS NOT NULL DROP TABLE core.UserBusinessRoles;
IF OBJECT_ID('core.UserSubscriptions', 'U') IS NOT NULL DROP TABLE core.UserSubscriptions;
IF OBJECT_ID('core.SubscriptionPlanPermissions', 'U') IS NOT NULL DROP TABLE core.SubscriptionPlanPermissions;
IF OBJECT_ID('core.RolePermissions', 'U') IS NOT NULL DROP TABLE core.RolePermissions;
IF OBJECT_ID('core.Permissions', 'U') IS NOT NULL DROP TABLE core.Permissions;
IF OBJECT_ID('core.Roles', 'U') IS NOT NULL DROP TABLE core.Roles;
IF OBJECT_ID('core.MenuItems', 'U') IS NOT NULL DROP TABLE core.MenuItems;
IF OBJECT_ID('core.Modules', 'U') IS NOT NULL DROP TABLE core.Modules;
IF OBJECT_ID('core.SubscriptionPlans', 'U') IS NOT NULL DROP TABLE core.SubscriptionPlans;
IF OBJECT_ID('core.Businesses', 'U') IS NOT NULL DROP TABLE core.Businesses;
IF OBJECT_ID('core.Users', 'U') IS NOT NULL DROP TABLE core.Users;
IF OBJECT_ID('Admin.OneTimePasswords', 'U') IS NOT NULL DROP TABLE Admin.OneTimePasswords;
GO

-- =================================================================================================
-- Section 1: Core Identity (Users & Businesses)
-- =================================================================================================

CREATE TABLE core.Users (
    UserID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserHandle NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(256) NULL,
    HashedPassword NVARCHAR(MAX) NULL,
    MobileCountryCode NVARCHAR(10) NULL,
    MobileNumber NVARCHAR(20) NULL,
    PanCardNumber_Encrypted VARBINARY(MAX) NOT NULL,
    AadharNumber_Encrypted VARBINARY(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAtUTC DATETIME2(7)  NULL 
);
-- Add unique constraints
CREATE UNIQUE INDEX UQ_Users_UserHandle ON core.Users(UserHandle);
CREATE UNIQUE INDEX UQ_Users_Email ON core.Users(Email) WHERE Email IS NOT NULL;
CREATE UNIQUE INDEX UQ_Users_Mobile ON core.Users(MobileCountryCode, MobileNumber) WHERE MobileNumber IS NOT NULL;
ALTER TABLE core.Users ADD PanCardNumberHash AS HASHBYTES('SHA2_256', PanCardNumber_Encrypted);
CREATE UNIQUE INDEX UQ_Users_PanCardNumberHash ON core.Users(PanCardNumberHash);
GO

CREATE TABLE core.Businesses (
    BusinessID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessName NVARCHAR(250) NOT NULL,
    BusinessCode NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Gstin NVARCHAR(15) NULL,
    TanNumber NVARCHAR(10) NULL,
    AddressLine1 NVARCHAR(250) NULL,
    AddressLine2 NVARCHAR(250) NULL,
    City NVARCHAR(100) NULL,
    StateID INT NULL,
    PinCode NVARCHAR(10) NULL,
    CountryID INT NULL,
    CreatedByUserID UNIQUEIDENTIFIER NOT NULL,
    CreatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedByUserID UNIQUEIDENTIFIER NOT NULL,
    UpdatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Businesses_CreatedBy FOREIGN KEY (CreatedByUserID) REFERENCES core.Users(UserID),
    CONSTRAINT FK_Businesses_UpdatedBy FOREIGN KEY (UpdatedByUserID) REFERENCES core.Users(UserID)
);
CREATE UNIQUE INDEX UQ_Businesses_BusinessCode ON core.Businesses(BusinessCode);
GO


-- =================================================================================================
-- Section 2: Subscription and Plan Management (CORRECTED)
-- =================================================================================================

CREATE TABLE core.SubscriptionPlans (
    PlanID INT PRIMARY KEY IDENTITY(1,1),
    PlanName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18, 2) NOT NULL DEFAULT 0, -- CORRECTED: Single price column
    MaxUsers INT NOT NULL,                     -- CORRECTED: Added column
    MaxBusinesses INT NOT NULL,                -- CORRECTED: Added column
    IsPubliclyVisible BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE core.UserSubscriptions (
    SubscriptionID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    PlanID INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    StartDateUTC DATETIME2(7) NOT NULL,
    EndDateUTC DATETIME2(7) NOT NULL,
    TrialEndsAtUTC DATETIME2(7) NULL,
    CreatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_UserSubscriptions_BusinessID FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID) ON DELETE CASCADE,
    CONSTRAINT FK_UserSubscriptions_PlanID FOREIGN KEY (PlanID) REFERENCES core.SubscriptionPlans(PlanID)
);
GO


-- =================================================================================================
-- Section 3: RBAC (Role-Based Access Control) (CORRECTED)
-- =================================================================================================

CREATE TABLE core.Modules (
    ModuleID INT PRIMARY KEY IDENTITY(1,1),
    ModuleName NVARCHAR(100) NOT NULL,
    IsSystemModule BIT NOT NULL DEFAULT 0, -- CORRECTED: Added column
    IsActive BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE core.MenuItems (
    MenuItemID INT PRIMARY KEY IDENTITY(1,1),
    ModuleID INT NOT NULL,
    ParentMenuItemID INT NULL,
    MenuText NVARCHAR(100) NOT NULL,
    MenuURL NVARCHAR(250) NULL,
    IconClass NVARCHAR(100) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_MenuItems_ModuleID FOREIGN KEY (ModuleID) REFERENCES core.Modules(ModuleID),
    CONSTRAINT FK_MenuItems_ParentMenuItemID FOREIGN KEY (ParentMenuItemID) REFERENCES core.MenuItems(MenuItemID)
);
GO

CREATE TABLE core.Permissions (
    PermissionID INT PRIMARY KEY IDENTITY(1,1),
    PermissionKey NVARCHAR(100) NOT NULL, -- CORRECTED: Renamed from PermissionCode
    Description NVARCHAR(500) NOT NULL,
    MenuItemID INT NULL,
    ModuleID INT NOT NULL,

    CONSTRAINT FK_Permissions_MenuItemID FOREIGN KEY (MenuItemID) REFERENCES core.MenuItems(MenuItemID),
    CONSTRAINT FK_Permissions_ModuleID FOREIGN KEY (ModuleID) REFERENCES core.Modules(ModuleID)
);
CREATE UNIQUE INDEX UQ_Permissions_PermissionKey ON core.Permissions(PermissionKey);
GO

CREATE TABLE core.Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0
);
GO

-- Junction table to map Permissions to Roles
CREATE TABLE core.RolePermissions (
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (RoleID, PermissionID),
    CONSTRAINT FK_RolePermissions_RoleID FOREIGN KEY (RoleID) REFERENCES core.Roles(RoleID) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_PermissionID FOREIGN KEY (PermissionID) REFERENCES core.Permissions(PermissionID) ON DELETE CASCADE
);
GO

-- Junction table to map Permissions to Subscription Plans
CREATE TABLE core.SubscriptionPlanPermissions (
    PlanID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (PlanID, PermissionID),
    CONSTRAINT FK_SubscriptionPlanPermissions_PlanID FOREIGN KEY (PlanID) REFERENCES core.SubscriptionPlans(PlanID) ON DELETE CASCADE,
    CONSTRAINT FK_SubscriptionPlanPermissions_PermissionID FOREIGN KEY (PermissionID) REFERENCES core.Permissions(PermissionID) ON DELETE CASCADE
);
GO

-- Junction table to map a User to a Role within a specific Business
CREATE TABLE core.UserBusinessRoles (
    UserID UNIQUEIDENTIFIER NOT NULL,
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, BusinessID, RoleID),
    CONSTRAINT FK_UserBusinessRoles_UserID FOREIGN KEY (UserID) REFERENCES core.Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_UserBusinessRoles_BusinessID FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID) ON DELETE CASCADE,
    CONSTRAINT FK_UserBusinessRoles_RoleID FOREIGN KEY (RoleID) REFERENCES core.Roles(RoleID) ON DELETE CASCADE
);
GO

-- =================================================================================================
-- Section 4: Seed Initial Data
-- =================================================================================================
PRINT 'Seeding initial system data...';
GO

-- Seed default system roles
INSERT INTO core.Roles (RoleName, Description, IsSystemRole) VALUES
('Business Owner', 'Has ultimate control over the business account, including subscription and billing.', 1),
('Admin', 'Can manage users and has full access to all modules within the business.', 1),
('Accountant', 'Has access to the Accounts module for managing transactions, ledgers, and reports.', 0),
('Sales Staff', 'Has access to CRM and Point of Sale (POS) modules.', 0),
('Read Only', 'Can view data and reports but cannot create or edit records.', 0);
GO

PRINT 'Core database schema created and seeded successfully.';
GO



-- Create schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Admin')
BEGIN
    EXEC('CREATE SCHEMA Admin');
END
GO

-- Create the table for storing one-time passwords
CREATE TABLE Admin.OneTimePasswords (
    OtpID BIGINT IDENTITY(1,1) PRIMARY KEY,
    LoginIdentifier NVARCHAR(100) NOT NULL,
    OTP NVARCHAR(6) NOT NULL,
    ExpiryTimeUTC DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAtUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Add an index for faster lookups on the login identifier
CREATE INDEX IX_OneTimePasswords_LoginIdentifier ON Admin.OneTimePasswords(LoginIdentifier);
GO

