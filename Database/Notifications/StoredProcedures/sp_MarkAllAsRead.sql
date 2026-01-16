CREATE OR ALTER PROCEDURE sp_MarkAllAsRead
    @ReadAt DATETIME NULL,
    @UpdatedAt DATETIME NULL,
    @UserId NVARCHAR(100)
AS 
BEGIN 
    UPDATE Notifications
    SET IsRead = 1, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId AND IsRead = 0;
END