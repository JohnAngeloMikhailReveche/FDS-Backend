-- =============================================
-- Users Database Objects
-- =============================================
-- This file contains all views and stored procedures for the Users module
-- Execute this file to create/update all user-related database objects
-- =============================================

-- =============================================
-- VIEWS
-- =============================================
USE NotificationDB;
GO

CREATE OR ALTER VIEW dbo.vw_GetAllUsers
AS 
SELECT * 
FROM Users;

GO

-- =============================================
-- STORED PROCEDURES - Query Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_GetAllUsers
AS 
BEGIN 
    SELECT * FROM dbo.vw_GetAllUsers;
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_GetUserById
    @Id NVARCHAR(100) 
AS
BEGIN
    SELECT * FROM dbo.vw_GetAllUsers
    WHERE Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_GetUserContact
    @Id NVARCHAR(100)
AS
BEGIN
    SELECT Email, PhoneNumber 
    FROM dbo.vw_GetAllUsers
    WHERE Id = @Id;
END

GO

-- =============================================
-- STORED PROCEDURES - Command Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_CreateUser
    @Id NVARCHAR(100),
    @Email NVARCHAR(100),
    @PhoneNumber NVARCHAR(100)
AS
BEGIN
    INSERT INTO dbo.Users (Id, Email, PhoneNumber)
    VALUES (@Id, @Email, @PhoneNumber);
END

GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateUser
    @Id NVARCHAR(100),
    @Email NVARCHAR(100),
    @PhoneNumber NVARCHAR(100)
AS
BEGIN
    UPDATE dbo.Users
    SET Email = COALESCE(@Email, Email),
        PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber)
    WHERE Id = @Id;
END

GO

-- =============================================
-- STORED PROCEDURES - Delete Operations
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_DeleteUser
    @Id NVARCHAR(100)
AS
BEGIN
    DELETE FROM dbo.Users
    WHERE Id = @Id;
END

GO
