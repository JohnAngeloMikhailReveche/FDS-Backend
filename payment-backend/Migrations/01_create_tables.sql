-- =====================================================
-- Migration 01: Create Tables
-- Payment Service 2
-- =====================================================

-- Wallets Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
BEGIN
    CREATE TABLE Wallets (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(100) NOT NULL UNIQUE,
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Coins INT NOT NULL DEFAULT 0,
        LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

-- Transactions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        Id NVARCHAR(50) PRIMARY KEY,
        UserId NVARCHAR(100) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(500),
        ReferenceId NVARCHAR(100),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

-- Orders Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id NVARCHAR(50) PRIMARY KEY,
        UserId NVARCHAR(100) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        PaymentMethod NVARCHAR(50),
        PaymentStatus NVARCHAR(50) DEFAULT 'pending',
        VoucherCode NVARCHAR(50),
        VoucherDiscount DECIMAL(18,2) DEFAULT 0,
        CoinsUsed INT DEFAULT 0,
        CoinsDiscount DECIMAL(18,2) DEFAULT 0,
        FinalAmount DECIMAL(18,2) NOT NULL,
        Branch NVARCHAR(200),
        PaymentUrl NVARCHAR(MAX),
        PaymentLinkId NVARCHAR(100),
        CheckoutSessionId NVARCHAR(100),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CompletedAt DATETIME2
    );
END

-- OrderItems Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        FOREIGN KEY (OrderId) REFERENCES Orders(Id)
    );
END

-- TopUps Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TopUps')
BEGIN
    CREATE TABLE TopUps (
        Id NVARCHAR(50) PRIMARY KEY,
        UserId NVARCHAR(100) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        PaymentMethod NVARCHAR(50),
        PaymentUrl NVARCHAR(MAX),
        CheckoutSessionId NVARCHAR(100),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CompletedAt DATETIME2
    );
END

-- Vouchers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vouchers')
BEGIN
    CREATE TABLE Vouchers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        DiscountType NVARCHAR(20) NOT NULL,
        DiscountValue DECIMAL(18,2) NOT NULL,
        MinOrderAmount DECIMAL(18,2) DEFAULT 0,
        MaxDiscount DECIMAL(18,2),
        UsageLimit INT,
        UsedCount INT DEFAULT 0,
        ExpiresAt DATETIME2,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

-- Refunds Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Refunds')
BEGIN
    CREATE TABLE Refunds (
        Id NVARCHAR(50) PRIMARY KEY,
        UserId NVARCHAR(100) NOT NULL,
        OrderId NVARCHAR(50),
        Amount DECIMAL(18,2) NOT NULL,
        Reason NVARCHAR(1000),
        Category NVARCHAR(100),
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        CustomerName NVARCHAR(200),
        CustomerEmail NVARCHAR(200),
        CustomerPhone NVARCHAR(50),
        PhotoPath NVARCHAR(500),
        AdminNotes NVARCHAR(1000),
        RejectionReason NVARCHAR(500),
        ReviewedBy NVARCHAR(100),
        WalletCredited BIT DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ReviewedAt DATETIME2
    );
END

-- Add PhotoPath column if it doesn't exist (for existing databases)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Refunds') AND name = 'PhotoPath')
BEGIN
    ALTER TABLE Refunds ADD PhotoPath NVARCHAR(500);
END
