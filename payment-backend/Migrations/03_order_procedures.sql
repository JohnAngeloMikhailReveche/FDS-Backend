-- =====================================================
-- Migration 03: Order Stored Procedures
-- Payment Service 2
-- =====================================================

CREATE OR ALTER PROCEDURE SP_CreateOrder
    @UserId NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @PaymentMethod NVARCHAR(50),
    @VoucherCode NVARCHAR(50) = NULL,
    @VoucherDiscount DECIMAL(18,2) = 0,
    @CoinsUsed INT = 0,
    @CoinsDiscount DECIMAL(18,2) = 0,
    @Branch NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OrderId NVARCHAR(50) = 'ord_' + REPLACE(NEWID(), '-', '');
    DECLARE @FinalAmount DECIMAL(18,2) = @Amount - @VoucherDiscount - @CoinsDiscount;
    
    INSERT INTO Orders (Id, UserId, Amount, Status, PaymentMethod, PaymentStatus, 
                        VoucherCode, VoucherDiscount, CoinsUsed, CoinsDiscount, 
                        FinalAmount, Branch, CreatedAt)
    VALUES (@OrderId, @UserId, @Amount, 'pending', @PaymentMethod, 'pending',
            @VoucherCode, @VoucherDiscount, @CoinsUsed, @CoinsDiscount,
            @FinalAmount, @Branch, SYSUTCDATETIME());
    
    SELECT @OrderId AS OrderId;
END
GO

CREATE OR ALTER PROCEDURE SP_AddOrderItem
    @OrderId NVARCHAR(50),
    @Name NVARCHAR(200),
    @Quantity INT,
    @Price DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO OrderItems (OrderId, Name, Quantity, Price)
    VALUES (@OrderId, @Name, @Quantity, @Price);
END
GO

CREATE OR ALTER PROCEDURE SP_GetOrder
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Id, UserId, Amount, Status, PaymentMethod, PaymentStatus,
           VoucherCode, VoucherDiscount, CoinsUsed, CoinsDiscount,
           FinalAmount, Branch, PaymentUrl, PaymentLinkId, CheckoutSessionId,
           CreatedAt, CompletedAt
    FROM Orders
    WHERE Id = @OrderId;
    
    SELECT Id, OrderId, Name, Quantity, Price
    FROM OrderItems
    WHERE OrderId = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE SP_GetOrderItems
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Id, OrderId, Name, Quantity, Price
    FROM OrderItems
    WHERE OrderId = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE SP_GetOrdersByUser
    @UserId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT Id, UserId, Amount, Status, PaymentMethod, PaymentStatus,
           VoucherCode, VoucherDiscount, CoinsUsed, CoinsDiscount,
           FinalAmount, Branch, PaymentUrl, PaymentLinkId, CheckoutSessionId,
           CreatedAt, CompletedAt
    FROM Orders
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE SP_CompleteOrder
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId NVARCHAR(100), @Amount DECIMAL(18,2), @PaymentMethod NVARCHAR(50), @Status NVARCHAR(50);
    SELECT @UserId = UserId, @Amount = FinalAmount, @PaymentMethod = PaymentMethod, @Status = Status
    FROM Orders WHERE Id = @OrderId;

    IF @Status = 'completed' RETURN;

    BEGIN TRANSACTION;

    UPDATE Orders
    SET Status = 'completed',
        PaymentStatus = 'completed',
        CompletedAt = SYSUTCDATETIME()
    WHERE Id = @OrderId;

    IF @PaymentMethod != 'wallet'
    BEGIN
        DECLARE @TxnId NVARCHAR(50) = 'txn_' + REPLACE(NEWID(), '-', '');
        INSERT INTO Transactions (Id, UserId, Type, Amount, Description, ReferenceId, CreatedAt)
        VALUES (@TxnId, @UserId, 'order', -@Amount, 'Order - External Payment', @OrderId, SYSUTCDATETIME());
    END
    
    COMMIT;

    SELECT Id, UserId, Amount, Status, PaymentMethod, PaymentStatus,
           FinalAmount, CreatedAt, CompletedAt
    FROM Orders
    WHERE Id = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE SP_UpdateOrderPaymentData
    @OrderId NVARCHAR(50),
    @PaymentUrl NVARCHAR(MAX) = NULL,
    @PaymentLinkId NVARCHAR(100) = NULL,
    @CheckoutSessionId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Orders
    SET PaymentUrl = @PaymentUrl,
        PaymentLinkId = @PaymentLinkId,
        CheckoutSessionId = @CheckoutSessionId
    WHERE Id = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE SP_UpdateOrder
    @OrderId NVARCHAR(50),
    @PaymentMethod NVARCHAR(50),
    @VoucherCode NVARCHAR(50) = NULL,
    @VoucherDiscount DECIMAL(18,2) = 0,
    @CoinsDiscount DECIMAL(18,2) = 0,
    @FinalAmount DECIMAL(18,2),
    @Status NVARCHAR(50),
    @PaymentUrl NVARCHAR(MAX) = NULL,
    @PaymentLinkId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Orders
    SET PaymentMethod = @PaymentMethod,
        VoucherCode = @VoucherCode,
        VoucherDiscount = @VoucherDiscount,
        CoinsDiscount = @CoinsDiscount,
        FinalAmount = @FinalAmount,
        Status = @Status,
        PaymentUrl = @PaymentUrl,
        PaymentLinkId = @PaymentLinkId
    WHERE Id = @OrderId;
END
GO
