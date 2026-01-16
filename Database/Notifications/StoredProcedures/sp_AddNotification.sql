CREATE OR ALTER PROCEDURE sp_AddNotification
    @UserId NVARCHAR(100),
    @Type NVARCHAR(100),
    @Subject NVARCHAR(100),
    @Body NVARCHAR(100),
    @IsRead INT,
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME NULL
AS 
BEGIN
    INSERT INTO Notifications(UserId, Type, Subject, Body, IsRead, CreatedAt, UpdatedAt)
    VALUES(@UserId, @Type, @Subject, @Body, @IsRead, @CreatedAt, @UpdatedAt);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END