-- =====================================================
-- Migration 04: TopUp Stored Procedures
-- Payment Service 2
-- =====================================================

CREATE OR ALTER PROCEDURE SP_CreateTopUp
    @UserId NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @PaymentMethod NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TopUpId NVARCHAR(50) = 'top_' + REPLACE(NEWID(), '-', '');
    
    INSERT INTO TopUps (Id, UserId, Amount, Status, PaymentMethod, CreatedAt)
    VALUES (@TopUpId, @UserId, @Amount, 'pending', @PaymentMethod, SYSUTCDATETIME());
    
    SELECT Id, UserId, Amount, Status, PaymentMethod, PaymentUrl, CreatedAt
    FROM TopUps
    WHERE Id = @TopUpId;
END
GO

CREATE OR ALTER PROCEDURE SP_GetTopUp
    @TopUpId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Id, UserId, Amount, Status, PaymentMethod, PaymentUrl, CheckoutSessionId, CreatedAt, CompletedAt
    FROM TopUps
    WHERE Id = @TopUpId;
END
GO

CREATE OR ALTER PROCEDURE SP_UpdateTopUpPaymentUrl
    @TopUpId NVARCHAR(50),
    @PaymentUrl NVARCHAR(MAX),
    @CheckoutSessionId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE TopUps
    SET PaymentUrl = @PaymentUrl,
        CheckoutSessionId = @CheckoutSessionId
    WHERE Id = @TopUpId;
END
GO

CREATE OR ALTER PROCEDURE SP_CompleteTopUp
    @TopUpId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId NVARCHAR(100);
    DECLARE @Amount DECIMAL(18,2);
    
    SELECT @UserId = UserId, @Amount = Amount
    FROM TopUps
    WHERE Id = @TopUpId AND Status = 'pending';
    
    IF @UserId IS NULL
    BEGIN
        RAISERROR('TopUp not found or already processed', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    
    UPDATE TopUps
    SET Status = 'completed',
        CompletedAt = SYSUTCDATETIME()
    WHERE Id = @TopUpId;
    
    EXEC SP_AddBalance @UserId, @Amount, @TopUpId, 'Wallet Top-up', 'topup';
    
    COMMIT;
    
    SELECT Id, UserId, Amount, Status, PaymentMethod, CreatedAt, CompletedAt
    FROM TopUps
    WHERE Id = @TopUpId;
END
GO
