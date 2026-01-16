CREATE OR ALTER PROCEDURE sp_MarkAsRead
    @ReadAt DATETIME NULL,
    @UpdatedAt DATETIME NULL,
    @Id INT,
    @UserId NVARCHAR(100)
AS
BEGIN
    UPDATE Notifications
    SET IsRead = 1, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt
    WHERE Id = @Id AND UserId = @UserId;
END