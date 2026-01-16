CREATE OR ALTER PROCEDURE sp_UpdateNotification
    @Type NVARCHAR(100),
    @Subject NVARCHAR(100),
    @Body NVARCHAR(100),
    @IsRead INT,
    @ReadAt DATETIME,
    @UpdatedAt DATETIME,
    @Id INT,
    @UserId NVARCHAR(100)
AS
BEGIN
    UPDATE Notifications
    SET Type = @Type,
        Subject = @Subject,
        Body = @Body,
        IsRead = @IsRead,
        ReadAt = @ReadAt,
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id AND UserId = @UserId;
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
