-- =====================================================
-- Migration 07: Seed Data
-- Payment Service 2
-- =====================================================

-- Seed Vouchers
IF NOT EXISTS (SELECT 1 FROM Vouchers WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO Vouchers (Code, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscount, IsActive)
    VALUES 
        ('WELCOME10', 'Welcome discount - 10% off', 'percentage', 10, 100, 50, 1),
        ('FLAT50', 'Flat 50 off on orders above 500', 'fixed', 50, 500, NULL, 1),
        ('SAVE20', '20% off up to 100 discount', 'percentage', 20, 200, 100, 1);
END

-- Seed Test Order
IF NOT EXISTS (SELECT 1 FROM Orders WHERE Id LIKE 'ord_coffee_%')
BEGIN
    DECLARE @OrderId NVARCHAR(50) = 'ord_coffee_' + REPLACE(NEWID(), '-', '');
    DECLARE @UserId NVARCHAR(100) = 'user_001';

    INSERT INTO Orders (Id, UserId, Amount, Status, PaymentMethod, PaymentStatus, FinalAmount, Branch, CreatedAt)
    VALUES (@OrderId, @UserId, 440.00, 'pending', 'wallet', 'pending', 440.00, 'Kapebara Main', SYSUTCDATETIME());

    INSERT INTO OrderItems (OrderId, Name, Quantity, Price)
    VALUES 
    (@OrderId, 'Caramel Macchiato', 2, 160.00),
    (@OrderId, 'Ham & Cheese Croissant', 1, 120.00);
END
