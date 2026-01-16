CREATE OR ALTER PROCEDURE sp_CreateUser
    @Id NVARCHAR(100),
    @Email NVARCHAR(100),
    @PhoneNumber NVARCHAR(100)
AS
BEGIN
    INSERT INTO Users (Id, Email, PhoneNumber)
    VALUES (@Id, @Email, @PhoneNumber);
END
