-- This script creates the Users table within the Admin schema.
-- It assumes the 'Admin' schema already exists.
-- If not, you must run: CREATE SCHEMA Admin;

CREATE TABLE Admin.Users (
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