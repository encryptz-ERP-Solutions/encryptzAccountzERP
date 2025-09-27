-- This script creates all necessary tables for the refactored API and new features.
-- Please execute this script against your database.

-- Create schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
BEGIN
    EXEC('CREATE SCHEMA core');
END
GO

-- 1. Create the Users table
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

-- 2. Create the OneTimePasswords table
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

-- Add an index for faster lookups on the login identifier
CREATE INDEX IX_OneTimePasswords_LoginIdentifier ON core.OneTimePasswords(LoginIdentifier);
GO
PRINT 'Table core.OneTimePasswords created successfully.';

PRINT 'Database schema setup is complete.';