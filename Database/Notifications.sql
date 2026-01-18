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
GO

CREATE OR ALTER VIEW dbo.vw_GetAllNotifications
AS
SELECT * FROM dbo.Notifications;

GO

-- =============================================
-- STORED PROCEDURES - Query Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_GetAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    SELECT * FROM dbo.vw_GetAllNotifications
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_GetNotification
    @UserId NVARCHAR(101),
    @Id INT
AS 
BEGIN 
    SELECT * FROM dbo.vw_GetAllNotifications
    WHERE UserId = @UserId AND Id = @Id;
END

GO

-- =============================================
-- STORED PROCEDURES - Command Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_AddNotification
    @UserId NVARCHAR(100),
    @Type NVARCHAR(100),
    @Subject NVARCHAR(100),
    @Body NVARCHAR(100),
    @IsRead INT,
    @CreatedAt DATETIME,
    @ReadAt DATETIME NULL
AS 
BEGIN
    INSERT INTO dbo.Notifications(UserId, Type, Subject, Body, IsRead, CreatedAt, ReadAt)
    VALUES(@UserId, @Type, @Subject, @Body, @IsRead, @CreatedAt, @ReadAt);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END

GO

-- =============================================
-- STORED PROCEDURES - Mark as Read Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_MarkAsRead
    @ReadAt DATETIME NULL,
    @Id INT,
    @UserId NVARCHAR(100)
AS
BEGIN
    UPDATE dbo.Notifications
    SET IsRead = 1, ReadAt = @ReadAt
    WHERE Id = @Id AND UserId = @UserId;
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_MarkAllAsRead
    @ReadAt DATETIME NULL,
    @UserId NVARCHAR(100)
AS 
BEGIN 
    UPDATE dbo.Notifications
    SET IsRead = 1, ReadAt = @ReadAt
    WHERE UserId = @UserId AND IsRead = 0;
END

GO

-- =============================================
-- STORED PROCEDURES - Delete Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_DeleteNotification
    @UserId NVARCHAR(100),
    @Id INT
AS
BEGIN 
    DELETE FROM dbo.Notifications
    WHERE UserId = @UserId AND Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    DELETE FROM dbo.Notifications
    WHERE UserId = @UserId;
END

GO
