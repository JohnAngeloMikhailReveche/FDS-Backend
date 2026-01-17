-- =============================================
-- Notifications Database Objects
-- =============================================
-- This file contains all views and stored procedures for the Notifications module
-- Execute this file to create/update all notification-related database objects
-- =============================================

-- =============================================
-- VIEWS
-- =============================================
USE NotificationDB;

CREATE OR ALTER VIEW vw_GetAllNotifications
AS
SELECT * FROM Notifications;

GO

-- =============================================
-- STORED PROCEDURES - Query Operations
-- =============================================

CREATE OR ALTER PROCEDURE sp_GetAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    SELECT * FROM vw_GetAllNotifications
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE sp_GetNotification
    @UserId NVARCHAR(101),
    @Id INT
AS 
BEGIN 
    SELECT * FROM vw_GetAllNotifications
    WHERE UserId = @UserId AND Id = @Id;
END

GO

-- =============================================
-- STORED PROCEDURES - Command Operations
-- =============================================

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

GO

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

GO

-- =============================================
-- STORED PROCEDURES - Mark as Read Operations
-- =============================================

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

GO

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

GO

-- =============================================
-- STORED PROCEDURES - Delete Operations
-- =============================================

CREATE OR ALTER PROCEDURE sp_DeleteNotification
    @UserId NVARCHAR(100),
    @Id INT
AS
BEGIN 
    DELETE FROM Notifications
    WHERE UserId = @UserId AND Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE sp_DeleteAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    DELETE FROM Notifications
    WHERE UserId = @UserId;
END

GO
