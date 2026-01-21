-- ============================================================================
-- FOOD DELIVERY DB - ALL STORED PROCEDURES
-- Database: FoodDeliveryDB
-- Created: 1/22/2026
-- Description: Consolidated script containing all stored procedures
-- ============================================================================

USE [FoodDeliveryDB]
GO

-- ============================================================================
-- 1. SP_CheckEmailExists
-- Purpose: Check if email exists in any user table (Admins, Riders, Customers)
-- Parameters: @Email (NVARCHAR)
-- Returns: BIT (1 if exists, 0 if not)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_CheckEmailExists]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CheckEmailExists]
GO

CREATE PROCEDURE [dbo].[SP_CheckEmailExists]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Admins WHERE Email = @Email)
       OR EXISTS (SELECT 1 FROM Riders WHERE Email = @Email)
       OR EXISTS (SELECT 1 FROM Customers WHERE Email = @Email)
    BEGIN
        SELECT CAST(1 AS BIT) AS EmailExists;
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS BIT) AS EmailExists;
    END
END
GO

-- ============================================================================
-- 2. SP_LoginUser
-- Purpose: Retrieve user by email from Admins/Riders/Customers tables
-- Parameters: @Email (NVARCHAR)
-- Returns: User data (Id, FullName, Email, PasswordHash, Role, IsActive)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_LoginUser]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_LoginUser]
GO

CREATE PROCEDURE [dbo].[SP_LoginUser]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    -- Search in Admins table
    SELECT 
        Id,
        FullName,
        Email,
        PasswordHash,
        Role,
        IsActive
    FROM Admins
    WHERE Email = @Email AND IsActive = 1
    UNION ALL
    -- Search in Riders table
    SELECT 
        Id,
        FullName,
        Email,
        PasswordHash,
        Role,
        IsActive
    FROM Riders
    WHERE Email = @Email AND IsActive = 1
    UNION ALL
    -- Search in Customers table
    SELECT 
        Id,
        FullName,
        Email,
        PasswordHash,
        Role,
        IsActive
    FROM Customers
    WHERE Email = @Email AND IsActive = 1;
END
GO

-- ============================================================================
-- 3. SP_RegisterCustomer
-- Purpose: Insert new customer into Customers table
-- Parameters: @FullName, @Email, @PasswordHash, @PhoneNumber, @Address
-- Returns: Newly created customer data (Id, Email, Role)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_RegisterCustomer]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_RegisterCustomer]
GO

CREATE PROCEDURE [dbo].[SP_RegisterCustomer]
    @FullName NVARCHAR(255),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(MAX),
    @PhoneNumber NVARCHAR(50) = NULL,
    @Address NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Role INT = 4; -- Customer role
    
    INSERT INTO Customers (Id, FullName, Email, PasswordHash, Role, PhoneNumber, Address, CreatedAt, IsActive)
    VALUES (@NewId, @FullName, @Email, @PasswordHash, @Role, @PhoneNumber, @Address, GETUTCDATE(), 1);
    
    -- Return the newly created customer
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive
    FROM Customers
    WHERE Id = @NewId;
END
GO

-- ============================================================================
-- 4. SP_CreateAdmin
-- Purpose: Insert new admin into Admins table
-- Parameters: @FullName, @Email, @PasswordHash
-- Returns: Newly created admin data (Id, Email, Role)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_CreateAdmin]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CreateAdmin]
GO

CREATE PROCEDURE [dbo].[SP_CreateAdmin]
    @FullName NVARCHAR(255),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Role INT = 2; -- Admin role
    
    INSERT INTO Admins (Id, FullName, Email, PasswordHash, Role, CreatedAt, IsActive)
    VALUES (@NewId, @FullName, @Email, @PasswordHash, @Role, GETUTCDATE(), 1);
    
    -- Return the newly created admin
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive
    FROM Admins
    WHERE Id = @NewId;
END
GO

-- ============================================================================
-- 5. SP_CreateRider
-- Purpose: Insert new rider into Riders table
-- Parameters: @FullName, @Email, @PasswordHash, @ContactNumber, @MotorcycleModel, @PlateNumber
-- Returns: Newly created rider data (Id, Email, Role, PlateNumber)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_CreateRider]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CreateRider]
GO

CREATE PROCEDURE [dbo].[SP_CreateRider]
    @FullName NVARCHAR(255),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(MAX),
    @ContactNumber NVARCHAR(50) = NULL,
    @MotorcycleModel NVARCHAR(100) = NULL,
    @PlateNumber NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    DECLARE @Role INT = 3; -- Rider role
    INSERT INTO Riders (Id, FullName, Email, PasswordHash, Role, ContactNumber, MotorcycleModel, PlateNumber, CreatedAt, IsActive)
    VALUES (@NewId, @FullName, @Email, @PasswordHash, @Role, @ContactNumber, @MotorcycleModel, @PlateNumber, GETUTCDATE(), 1);
    -- Return the newly created rider
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        PlateNumber,
        CreatedAt,
        IsActive
    FROM Riders
    WHERE Id = @NewId;
END
GO

-- ============================================================================
-- 6. SP_DeleteAccount
-- Purpose: Delete user from appropriate table (Admins/Riders/Customers)
-- Parameters: @Email (NVARCHAR)
-- Returns: @DeletedCount (INT OUTPUT), @UserType (NVARCHAR OUTPUT), @UserRole (INT OUTPUT)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_DeleteAccount]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_DeleteAccount]
GO

CREATE PROCEDURE [dbo].[SP_DeleteAccount]
    @Email NVARCHAR(255),
    @DeletedCount INT OUTPUT,
    @UserType NVARCHAR(50) OUTPUT,
    @UserRole INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @DeletedCount = 0;
    SET @UserType = 'Unknown';
    SET @UserRole = NULL;
    -- Try to delete from Admins table
    IF EXISTS (SELECT 1 FROM Admins WHERE Email = @Email)
    BEGIN
        SELECT @UserRole = Role FROM Admins WHERE Email = @Email;
        DELETE FROM Admins WHERE Email = @Email;
        SET @DeletedCount = @@ROWCOUNT;
        SET @UserType = 'Admin';
        RETURN;
    END
    -- Try to delete from Riders table
    IF EXISTS (SELECT 1 FROM Riders WHERE Email = @Email)
    BEGIN
        SELECT @UserRole = Role FROM Riders WHERE Email = @Email;
        DELETE FROM Riders WHERE Email = @Email;
        SET @DeletedCount = @@ROWCOUNT;
        SET @UserType = 'Rider';
        RETURN;
    END
    -- Try to delete from Customers table
    IF EXISTS (SELECT 1 FROM Customers WHERE Email = @Email)
    BEGIN
        SELECT @UserRole = Role FROM Customers WHERE Email = @Email;
        DELETE FROM Customers WHERE Email = @Email;
        SET @DeletedCount = @@ROWCOUNT;
        SET @UserType = 'Customer';
        RETURN;
    END
END
GO

-- ============================================================================
-- 7. SP_GetAllUsers
-- Purpose: Retrieve all users from all three tables
-- Parameters: None
-- Returns: Combined list of all users with UNION ALL
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_GetAllUsers]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GetAllUsers]
GO

CREATE PROCEDURE [dbo].[SP_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    -- Get all Admins
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        NULL AS ContactNumber,
        NULL AS MotorcycleModel,
        NULL AS PlateNumber,
        NULL AS PhoneNumber,
        NULL AS Address
    FROM Admins
    UNION ALL
    -- Get all Riders
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        ContactNumber,
        MotorcycleModel,
        PlateNumber,
        NULL AS PhoneNumber,
        NULL AS Address
    FROM Riders
    UNION ALL
    -- Get all Customers
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        NULL AS ContactNumber,
        NULL AS MotorcycleModel,
        NULL AS PlateNumber,
        PhoneNumber,
        Address
    FROM Customers
    ORDER BY Role, Email;
END
GO

-- ============================================================================
-- 8. SP_GetUserProfile
-- Purpose: Retrieve user profile by email from appropriate table
-- Parameters: @Email (NVARCHAR)
-- Returns: User profile data with role-specific fields
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_GetUserProfile]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GetUserProfile]
GO

CREATE PROCEDURE [dbo].[SP_GetUserProfile]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    -- Search in Admins table
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        NULL AS ContactNumber,
        NULL AS MotorcycleModel,
        NULL AS PlateNumber,
        NULL AS PhoneNumber,
        NULL AS Address,
        'Admin' AS UserType
    FROM Admins
    WHERE Email = @Email
    UNION ALL
    -- Search in Riders table
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        ContactNumber,
        MotorcycleModel,
        PlateNumber,
        NULL AS PhoneNumber,
        NULL AS Address,
        'Rider' AS UserType
    FROM Riders
    WHERE Email = @Email
    UNION ALL
    -- Search in Customers table
    SELECT 
        Id,
        FullName,
        Email,
        Role,
        CreatedAt,
        IsActive,
        NULL AS ContactNumber,
        NULL AS MotorcycleModel,
        NULL AS PlateNumber,
        PhoneNumber,
        Address,
        'Customer' AS UserType
    FROM Customers
    WHERE Email = @Email;
END
GO

-- ============================================================================
-- 9. SP_ResetPassword
-- Purpose: Reset user password (Forgot Password flow)
-- Parameters: @Email, @NewPasswordHash
-- Returns: UpdatedCount, UserType
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_ResetPassword]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ResetPassword]
GO

CREATE PROCEDURE [dbo].[SP_ResetPassword]
    @Email NVARCHAR(255),
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UpdatedCount INT = 0;
    DECLARE @UserType NVARCHAR(50) = NULL;
    
    -- Try to update in Admins
    IF EXISTS (SELECT 1 FROM Admins WHERE Email = @Email)
    BEGIN
        UPDATE Admins 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Admin';
    END
    -- Try to update in Riders
    ELSE IF EXISTS (SELECT 1 FROM Riders WHERE Email = @Email)
    BEGIN
        UPDATE Riders 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Rider';
    END
    -- Try to update in Customers
    ELSE IF EXISTS (SELECT 1 FROM Customers WHERE Email = @Email)
    BEGIN
        UPDATE Customers 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Customer';
    END
    
    SELECT @UpdatedCount AS UpdatedCount, @UserType AS UserType;
END
GO

-- ============================================================================
-- 10. SP_ChangePassword
-- Purpose: Change password for authenticated user
-- Parameters: @Email, @NewPasswordHash
-- Returns: UpdatedCount, UserType
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_ChangePassword]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ChangePassword]
GO

CREATE PROCEDURE [dbo].[SP_ChangePassword]
    @Email NVARCHAR(255),
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UpdatedCount INT = 0;
    DECLARE @UserType NVARCHAR(50) = NULL;
    
    -- Try to update in Admins
    IF EXISTS (SELECT 1 FROM Admins WHERE Email = @Email AND IsActive = 1)
    BEGIN
        UPDATE Admins 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Admin';
    END
    -- Try to update in Riders
    ELSE IF EXISTS (SELECT 1 FROM Riders WHERE Email = @Email AND IsActive = 1)
    BEGIN
        UPDATE Riders 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Rider';
    END
    -- Try to update in Customers
    ELSE IF EXISTS (SELECT 1 FROM Customers WHERE Email = @Email AND IsActive = 1)
    BEGIN
        UPDATE Customers 
        SET PasswordHash = @NewPasswordHash
        WHERE Email = @Email;
        
        SET @UpdatedCount = @@ROWCOUNT;
        SET @UserType = 'Customer';
    END
    
    SELECT @UpdatedCount AS UpdatedCount, @UserType AS UserType;
END
GO

-- ============================================================================
-- 11. SP_GetUserPasswordHash
-- Purpose: Get user's current password hash
-- Parameters: @Email
-- Returns: PasswordHash, UserType
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_GetUserPasswordHash]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GetUserPasswordHash]
GO

CREATE PROCEDURE [dbo].[SP_GetUserPasswordHash]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check Admins
    SELECT 
        PasswordHash,
        'Admin' AS UserType
    FROM Admins
    WHERE Email = @Email AND IsActive = 1
    
    UNION ALL
    
    -- Check Riders
    SELECT 
        PasswordHash,
        'Rider' AS UserType
    FROM Riders
    WHERE Email = @Email AND IsActive = 1
    
    UNION ALL
    
    -- Check Customers
    SELECT 
        PasswordHash,
        'Customer' AS UserType
    FROM Customers
    WHERE Email = @Email AND IsActive = 1;
END
GO

-- ============================================================================
-- 12. SP_UpdateUserProfile
-- Purpose: Update user profile information
-- Parameters: @Email, @FullName, @PhoneNumber, @Address
-- Returns: None (raises error if user not found)
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_UpdateUserProfile]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UpdateUserProfile]
GO

CREATE PROCEDURE [dbo].[SP_UpdateUserProfile]
    @Email NVARCHAR(255),
    @FullName NVARCHAR(200) = NULL,
    @PhoneNumber NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsAffected INT = 0;
    DECLARE @ErrorMessage NVARCHAR(4000);
    
    BEGIN TRY
        -- Try updating Customers table
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers')
        BEGIN
            UPDATE Customers
            SET 
                FullName = ISNULL(@FullName, FullName),
                PhoneNumber = @PhoneNumber,
                Address = @Address
            WHERE Email = @Email;
            
            SET @RowsAffected = @@ROWCOUNT;
        END
        -- Try Customer table (singular)
        ELSE IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customer')
        BEGIN
            UPDATE Customer
            SET 
                FullName = ISNULL(@FullName, FullName),
                PhoneNumber = @PhoneNumber,
                Address = @Address
            WHERE Email = @Email;
            
            SET @RowsAffected = @@ROWCOUNT;
        END
        
        -- If no customer found, try Admins table
        IF @RowsAffected = 0
        BEGIN
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Admins')
            BEGIN
                UPDATE Admins
                SET 
                    FullName = ISNULL(@FullName, FullName)
                WHERE Email = @Email;
                
                SET @RowsAffected = @@ROWCOUNT;
            END
            -- Try Admin table (singular)
            ELSE IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Admin')
            BEGIN
                UPDATE Admin
                SET 
                    FullName = ISNULL(@FullName, FullName)
                WHERE Email = @Email;
                
                SET @RowsAffected = @@ROWCOUNT;
            END
        END
        
        -- If no admin found, try Riders table
        IF @RowsAffected = 0
        BEGIN
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Riders')
            BEGIN
                UPDATE Riders
                SET 
                    FullName = ISNULL(@FullName, FullName),
                    ContactNumber = @PhoneNumber
                WHERE Email = @Email;
                
                SET @RowsAffected = @@ROWCOUNT;
            END
            -- Try Rider table (singular)
            ELSE IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rider')
            BEGIN
                UPDATE Rider
                SET 
                    FullName = ISNULL(@FullName, FullName),
                    ContactNumber = @PhoneNumber
                WHERE Email = @Email;
                
                SET @RowsAffected = @@ROWCOUNT;
            END
        END
        
        -- If still no user found, raise error
        IF @RowsAffected = 0
        BEGIN
            SET @ErrorMessage = 'User not found with email: ' + @Email;
            RAISERROR(@ErrorMessage, 16, 1);
            RETURN;
        END
        
    END TRY
    BEGIN CATCH
        -- Capture and re-throw the error
        SET @ErrorMessage = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN;
    END CATCH
END
GO

-- ============================================================================
-- 13. SP_ToggleUserStatus
-- Purpose: Toggle user active/inactive status
-- Parameters: @Email, @RequesterEmail, @RequesterRole
-- Returns: NewIsActive, UserRole, Email, FullName
-- ============================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SP_ToggleUserStatus]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ToggleUserStatus]
GO

CREATE PROCEDURE [dbo].[SP_ToggleUserStatus]
    @Email NVARCHAR(100),
    @RequesterEmail NVARCHAR(100),
    @RequesterRole INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Declare variables
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @UserRole INT;
    DECLARE @CurrentIsActive BIT;
    DECLARE @NewIsActive BIT;
    DECLARE @FullName NVARCHAR(100);
    DECLARE @UserType NVARCHAR(20);
    
    -- Debug logging
    PRINT '========================================';
    PRINT 'SP_ToggleUserStatus - Parameters:';
    PRINT '  @Email: ' + ISNULL(@Email, 'NULL');
    PRINT '  @RequesterEmail: ' + ISNULL(@RequesterEmail, 'NULL');
    PRINT '  @RequesterRole: ' + CAST(@RequesterRole AS NVARCHAR(10));
    PRINT '========================================';
    
    -- Try to find user in Customers table (Role = 2 or 4)
    SELECT 
        @UserId = Id,
        @UserRole = Role,
        @CurrentIsActive = IsActive,
        @FullName = FullName,
        @UserType = 'Customers'
    FROM Customers WITH (NOLOCK)
    WHERE Email = @Email;
    
    -- If not found, try Riders table (Role = 3)
    IF @UserId IS NULL
    BEGIN
        SELECT 
            @UserId = Id,
            @UserRole = Role,
            @CurrentIsActive = IsActive,
            @FullName = FullName,
            @UserType = 'Riders'
        FROM Riders WITH (NOLOCK)
        WHERE Email = @Email;
    END
    
    -- If not found, try Admins table (Role = 1 or 2)
    IF @UserId IS NULL
    BEGIN
        SELECT 
            @UserId = Id,
            @UserRole = Role,
            @CurrentIsActive = IsActive,
            @FullName = FullName,
            @UserType = 'Admins'
        FROM Admins WITH (NOLOCK)
        WHERE Email = @Email;
    END
    
    -- Check if user exists
    IF @UserId IS NULL
    BEGIN
        PRINT '❌ ERROR: User not found for email: ' + ISNULL(@Email, 'NULL');
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    PRINT '✅ User found:';
    PRINT '  UserId: ' + CAST(@UserId AS NVARCHAR(50));
    PRINT '  FullName: ' + ISNULL(@FullName, 'NULL');
    PRINT '  UserRole: ' + CAST(@UserRole AS NVARCHAR(10));
    PRINT '  CurrentIsActive: ' + CAST(@CurrentIsActive AS NVARCHAR(5));
    PRINT '  UserType (Table): ' + @UserType;
    
    -- Authorization checks
    -- Role values: 1=SuperAdmin, 2=Admin, 3=Rider, 4=Customer
    
    -- SuperAdmin (role 1) can toggle anyone except other SuperAdmins
    IF @RequesterRole = 1 -- SuperAdmin
    BEGIN
        PRINT 'Requester is SuperAdmin';
        IF @UserRole = 1 AND @Email != @RequesterEmail
        BEGIN
            PRINT '❌ ERROR: SuperAdmins cannot toggle other SuperAdmins';
            RAISERROR('SuperAdmins cannot toggle other SuperAdmins', 16, 1);
            RETURN;
        END
    END
    -- Admin (role 2) can toggle Customers and Riders only
    ELSE IF @RequesterRole = 2 -- Admin
    BEGIN
        PRINT 'Requester is Admin';
        IF @UserRole IN (1, 2) -- SuperAdmin or Admin
        BEGIN
            PRINT '❌ ERROR: Admins cannot toggle other Admins or SuperAdmins';
            RAISERROR('Admins cannot toggle other Admins or SuperAdmins', 16, 1);
            RETURN;
        END
    END
    ELSE
    BEGIN
        PRINT '❌ ERROR: Unauthorized role: ' + CAST(@RequesterRole AS NVARCHAR(10));
        RAISERROR('Unauthorized: Only Admins and SuperAdmins can toggle user status', 16, 1);
        RETURN;
    END
    
    -- Prevent self-deactivation
    IF @Email = @RequesterEmail
    BEGIN
        PRINT '❌ ERROR: Cannot toggle own status';
        RAISERROR('You cannot toggle your own status', 16, 1);
        RETURN;
    END
    
    -- Toggle the status
    SET @NewIsActive = CASE WHEN @CurrentIsActive = 1 THEN 0 ELSE 1 END;
    
    PRINT 'Toggling status:';
    PRINT '  From: ' + CAST(@CurrentIsActive AS NVARCHAR(5));
    PRINT '  To: ' + CAST(@NewIsActive AS NVARCHAR(5));
    
    -- Update in the appropriate table based on user type
    BEGIN TRY
        IF @UserType = 'Customers'
        BEGIN
            UPDATE Customers
            SET IsActive = @NewIsActive
            WHERE Id = @UserId;
            PRINT '✅ Status updated in Customers table';
        END
        ELSE IF @UserType = 'Riders'
        BEGIN
            UPDATE Riders
            SET IsActive = @NewIsActive
            WHERE Id = @UserId;
            PRINT '✅ Status updated in Riders table';
        END
        ELSE IF @UserType = 'Admins'
        BEGIN
            UPDATE Admins
            SET IsActive = @NewIsActive
            WHERE Id = @UserId;
            PRINT '✅ Status updated in Admins table';
        END
    END TRY
    BEGIN CATCH
        PRINT '❌ ERROR updating status:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN;
    END CATCH
    
    -- Return the result
    SELECT 
        @NewIsActive AS NewIsActive,
        @UserRole AS UserRole,
        @Email AS Email,
        @FullName AS FullName;
    
    PRINT '✅ Stored procedure completed successfully';
    PRINT '========================================';
END
GO

PRINT '============================================================================';
PRINT 'ALL STORED PROCEDURES CREATED SUCCESSFULLY';
PRINT 'Total Stored Procedures: 13';
PRINT '============================================================================';