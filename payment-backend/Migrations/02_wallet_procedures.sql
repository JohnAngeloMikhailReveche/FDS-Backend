-- =====================================================
-- Migration 02: Wallet Stored Procedures
-- Payment Service 2
-- =====================================================

CREATE OR ALTER PROCEDURE SP_GetWallet
    @UserId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Wallets WHERE UserId = @UserId)
    BEGIN
        INSERT INTO Wallets (UserId, Balance, Coins, LastUpdated)
        VALUES (@UserId, 0, 0, SYSUTCDATETIME());
    END
    
    SELECT Id, UserId, Balance, Coins, LastUpdated
    FROM Wallets
    WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE SP_AddBalance
    @UserId NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @ReferenceId NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @TransactionType NVARCHAR(50) = 'topup',
    @ReturnRecord BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    IF NOT EXISTS (SELECT 1 FROM Wallets WHERE UserId = @UserId)
    BEGIN
        INSERT INTO Wallets (UserId, Balance, Coins, LastUpdated)
        VALUES (@UserId, 0, 0, SYSUTCDATETIME());
    END
    
    DECLARE @CoinsEarned INT = FLOOR(@Amount / 100) * 5;
    
    UPDATE Wallets
    SET Balance = Balance + @Amount,
        Coins = Coins + @CoinsEarned,
        LastUpdated = SYSUTCDATETIME()
    WHERE UserId = @UserId;
    
    DECLARE @TxnId NVARCHAR(50) = 'txn_' + REPLACE(NEWID(), '-', '');
    DECLARE @ActualDesc NVARCHAR(500) = ISNULL(@Description, CASE WHEN @TransactionType = 'refund' THEN 'Refund for Order' ELSE 'Wallet Top-up' END);
    
    INSERT INTO Transactions (Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt)
    VALUES (@TxnId, @UserId, @TransactionType, @Amount, @ActualDesc, @ReferenceId, SYSUTCDATETIME());
    
    IF @CoinsEarned > 0
    BEGIN
        DECLARE @CoinsTxnId NVARCHAR(50) = 'txn_' + REPLACE(NEWID(), '-', '');
        INSERT INTO Transactions (Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt)
        VALUES (@CoinsTxnId, @UserId, 'coins', @CoinsEarned, 'Coins earned from top-up', @ReferenceId, SYSUTCDATETIME());
    END
    
    COMMIT;
    
    IF @ReturnRecord = 1
    BEGIN
        SELECT Id, UserId, Balance, Coins, LastUpdated
        FROM Wallets
        WHERE UserId = @UserId;
    END
END
GO

CREATE OR ALTER PROCEDURE SP_DeductBalance
    @UserId NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @ReferenceId NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentBalance DECIMAL(18,2);
    SELECT @CurrentBalance = Balance FROM Wallets WHERE UserId = @UserId;
    
    IF @CurrentBalance IS NULL OR @CurrentBalance < @Amount
    BEGIN
        RAISERROR('Insufficient balance', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    
    UPDATE Wallets
    SET Balance = Balance - @Amount,
        LastUpdated = SYSUTCDATETIME()
    WHERE UserId = @UserId;
    
    DECLARE @TxnId NVARCHAR(50) = 'txn_' + REPLACE(NEWID(), '-', '');
    INSERT INTO Transactions (Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt)
    VALUES (@TxnId, @UserId, 'order', -@Amount, ISNULL(@Description, 'Order Payment'), @ReferenceId, SYSUTCDATETIME());
    
    COMMIT;
    
    SELECT Id, UserId, Balance, Coins, LastUpdated
    FROM Wallets
    WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE SP_UseCoins
    @UserId NVARCHAR(100),
    @CoinsToUse INT,
    @ReferenceId NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @CoinsToUse <= 0
    BEGIN
        SELECT Id, UserId, Balance, Coins, LastUpdated
        FROM Wallets
        WHERE UserId = @UserId;
        RETURN;
    END
    
    DECLARE @CurrentCoins INT;
    SELECT @CurrentCoins = Coins FROM Wallets WHERE UserId = @UserId;
    
    IF @CurrentCoins IS NULL OR @CurrentCoins < @CoinsToUse
    BEGIN
        RAISERROR('Insufficient coins', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    
    UPDATE Wallets
    SET Coins = Coins - @CoinsToUse,
        LastUpdated = SYSUTCDATETIME()
    WHERE UserId = @UserId;
    
    DECLARE @TxnId NVARCHAR(50) = 'txn_' + REPLACE(NEWID(), '-', '');
    INSERT INTO Transactions (Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt)
    VALUES (@TxnId, @UserId, 'coins', -@CoinsToUse, ISNULL(@Description, 'Coins used'), @ReferenceId, SYSUTCDATETIME());
    
    COMMIT;
    
    SELECT Id, UserId, Balance, Coins, LastUpdated
    FROM Wallets
    WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE SP_GetTransactions
    @UserId NVARCHAR(100),
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit) Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt
    FROM Transactions
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END
GO
