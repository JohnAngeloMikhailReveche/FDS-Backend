CREATE OR ALTER PROCEDURE sp_UpdateUser
    @Id NVARCHAR(100),
    @Email NVARCHAR(100),
    @PhoneNumber NVARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET Email = COALESCE(@Email, Email),
        PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber)
    WHERE Id = @Id;
END