/*
================================================================================================
Filename: encryptzERP_Acct_Schema.sql
Description: Database schema for the Accounting Module of the Encryptz ERP.
Version: 1.0
================================================================================================
*/

-- Use your target database
USE encryptzERPCore;
GO

-- Create the 'Acct' schema if it doesn't already exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Acct')
BEGIN
    EXEC('CREATE SCHEMA Acct');
END
GO

-- =================================================================================================
-- Section 1: Chart of Accounts - The foundation of the accounting system.
-- =================================================================================================

-- Lookup table for the 5 main account types in accounting.
CREATE TABLE Acct.AccountTypes (
    AccountTypeID INT PRIMARY KEY IDENTITY(1,1),
    AccountTypeName NVARCHAR(50) NOT NULL, -- e.g., Asset, Liability, Equity, Revenue, Expense
    NormalBalance CHAR(2) NOT NULL -- 'Dr' for Debit, 'Cr' for Credit
);
GO

-- Seed the fundamental account types.
INSERT INTO Acct.AccountTypes (AccountTypeName, NormalBalance) VALUES
('Asset', 'Dr'),
('Liability', 'Cr'),
('Equity', 'Cr'),
('Revenue', 'Cr'),
('Expense', 'Dr');
GO

-- The Chart of Accounts for a specific business.
CREATE TABLE Acct.ChartOfAccounts (
    AccountID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    AccountTypeID INT NOT NULL,
    ParentAccountID UNIQUEIDENTIFIER NULL, -- For creating hierarchical accounts (e.g., 'Current Assets' is parent of 'Cash')
    AccountCode NVARCHAR(20) NOT NULL, -- e.g., '1010' for Cash
    AccountName NVARCHAR(200) NOT NULL, -- e.g., 'Cash in Bank'
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsSystemAccount BIT NOT NULL DEFAULT 0, -- Prevents deletion of critical accounts like 'Retained Earnings'
    CreatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAtUTC DATETIME2(7) NULL,

    CONSTRAINT FK_ChartOfAccounts_Business FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID) ON DELETE CASCADE,
    CONSTRAINT FK_ChartOfAccounts_AccountType FOREIGN KEY (AccountTypeID) REFERENCES Acct.AccountTypes(AccountTypeID),
    CONSTRAINT FK_ChartOfAccounts_ParentAccount FOREIGN KEY (ParentAccountID) REFERENCES Acct.ChartOfAccounts(AccountID),
    CONSTRAINT UQ_ChartOfAccounts_Business_AccountCode UNIQUE (BusinessID, AccountCode),
    CONSTRAINT UQ_ChartOfAccounts_Business_AccountName UNIQUE (BusinessID, AccountName)
);
GO

-- =================================================================================================
-- Section 2: Transaction Recording (Journal Entries)
-- =================================================================================================

-- Stores the master record for a single financial transaction (i.e., a journal entry).
CREATE TABLE Acct.TransactionHeaders (
    TransactionHeaderID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BusinessID UNIQUEIDENTIFIER NOT NULL,
    TransactionDate DATE NOT NULL,
    ReferenceNumber NVARCHAR(100) NULL, -- e.g., Invoice #, Cheque #
    Description NVARCHAR(500) NOT NULL,
    CreatedByUserID UNIQUEIDENTIFIER NOT NULL,
    CreatedAtUTC DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_TransactionHeaders_Business FOREIGN KEY (BusinessID) REFERENCES core.Businesses(BusinessID) ON DELETE CASCADE,
    CONSTRAINT FK_TransactionHeaders_User FOREIGN KEY (CreatedByUserID) REFERENCES core.Users(UserID)
);
GO

-- Stores the detail lines (debits and credits) for each transaction.
CREATE TABLE Acct.TransactionDetails (
    TransactionDetailID BIGINT PRIMARY KEY IDENTITY(1,1),
    TransactionHeaderID UNIQUEIDENTIFIER NOT NULL,
    AccountID UNIQUEIDENTIFIER NOT NULL,
    DebitAmount DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    CreditAmount DECIMAL(18, 2) NOT NULL DEFAULT 0.00,

    CONSTRAINT FK_TransactionDetails_Header FOREIGN KEY (TransactionHeaderID) REFERENCES Acct.TransactionHeaders(TransactionHeaderID) ON DELETE CASCADE,
    CONSTRAINT FK_TransactionDetails_Account FOREIGN KEY (AccountID) REFERENCES Acct.ChartOfAccounts(AccountID),
    -- IMPORTANT: This check ensures that a line is either a debit or a credit, but not both.
    CONSTRAINT CK_TransactionDetails_DebitOrCredit CHECK ( (DebitAmount > 0 AND CreditAmount = 0) OR (DebitAmount = 0 AND CreditAmount > 0) )
);
GO

PRINT 'Accounting module schema created successfully.';
GO