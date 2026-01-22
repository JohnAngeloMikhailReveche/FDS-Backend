-- =====================================================
-- Migration 06: Refund Stored Procedures
-- Payment Service 2
-- =====================================================

CREATE OR ALTER PROCEDURE SP_CreateRefund
    @UserId NVARCHAR(100),
    @OrderId NVARCHAR(50) = NULL,
    @Amount DECIMAL(18,2) = 0,
    @Reason NVARCHAR(1000) = NULL,
    @Category NVARCHAR(100) = NULL,
    @CustomerName NVARCHAR(200) = NULL,
    @CustomerEmail NVARCHAR(200) = NULL,
    @CustomerPhone NVARCHAR(50) = NULL,
    @PhotoPath NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @OrderId IS NOT NULL AND (@Amount IS NULL OR @Amount = 0)
    BEGIN
        SELECT @Amount = FinalAmount FROM Orders WHERE Id = @OrderId;
    END

    DECLARE @RefundId NVARCHAR(50) = 'ref_' + REPLACE(NEWID(), '-', '');
    
    INSERT INTO Refunds (Id, UserId, OrderId, Amount, Reason, Category, Status,
                         CustomerName, CustomerEmail, CustomerPhone, PhotoPath, CreatedAt)
    VALUES (@RefundId, @UserId, @OrderId, @Amount, @Reason, @Category, 'pending',
            @CustomerName, @CustomerEmail, @CustomerPhone, @PhotoPath, SYSUTCDATETIME());
    
    SELECT Id, UserId, OrderId, Amount, Reason, Category, Status,
           CustomerName, CustomerEmail, CustomerPhone, PhotoPath, CreatedAt
    FROM Refunds
    WHERE Id = @RefundId;
END
GO

CREATE OR ALTER PROCEDURE SP_GetRefunds
    @UserId NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.Id, r.UserId, r.OrderId, r.Amount, r.Reason, r.Category, r.Status,
           r.CustomerName, r.CustomerEmail, r.CustomerPhone, r.PhotoPath, r.AdminNotes,
           r.RejectionReason, r.ReviewedBy, r.WalletCredited, r.CreatedAt, r.ReviewedAt,
           o.VoucherCode, o.VoucherDiscount
    FROM Refunds r
    LEFT JOIN Orders o ON r.OrderId = o.Id
    WHERE (@UserId IS NULL OR r.UserId = @UserId)
      AND (@Status IS NULL OR r.Status = @Status)
    ORDER BY r.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE SP_ReviewRefund
    @RefundId NVARCHAR(50),
    @Action NVARCHAR(20),
    @AdminNotes NVARCHAR(1000) = NULL,
    @RejectionReason NVARCHAR(500) = NULL,
    @ReviewedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewStatus NVARCHAR(50);
    IF @Action = 'approve'
        SET @NewStatus = 'approved';
    ELSE
        SET @NewStatus = 'rejected';
    
    UPDATE Refunds
    SET Status = @NewStatus,
        AdminNotes = @AdminNotes,
        RejectionReason = @RejectionReason,
        ReviewedBy = @ReviewedBy,
        ReviewedAt = SYSUTCDATETIME()
    WHERE Id = @RefundId;
    
    SELECT Id, UserId, OrderId, Amount, Status, AdminNotes, RejectionReason,
           ReviewedBy, WalletCredited, ReviewedAt
    FROM Refunds
    WHERE Id = @RefundId;
END
GO

CREATE OR ALTER PROCEDURE SP_ProcessRefundToWallet
    @RefundId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId NVARCHAR(100), @Amount DECIMAL(18,2), @Status NVARCHAR(50), @WalletCredited BIT, @OrderId NVARCHAR(50);
    
    SELECT @UserId = UserId, @Amount = Amount, @Status = Status, @WalletCredited = WalletCredited, @OrderId = OrderId
    FROM Refunds
    WHERE Id = @RefundId;
    
    IF @Status != 'approved'
    BEGIN
        RAISERROR('Refund must be approved first', 16, 1);
        RETURN;
    END
    
    IF @WalletCredited = 1
    BEGIN
        RAISERROR('Refund already credited to wallet', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    
    DECLARE @RefundDesc NVARCHAR(500) = 'Refund for Order ' + ISNULL(@OrderId, '');

    EXEC SP_AddBalance @UserId, @Amount, @RefundId, @RefundDesc, 'refund', 0;
    
    UPDATE Refunds
    SET WalletCredited = 1,
        Status = 'completed'
    WHERE Id = @RefundId;
    
    COMMIT;
    
    SELECT Id, UserId, Amount, Status, WalletCredited
    FROM Refunds
    WHERE Id = @RefundId;
END
GO
