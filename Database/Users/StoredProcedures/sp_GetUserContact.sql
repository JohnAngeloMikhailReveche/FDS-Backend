CREATE OR ALTER PROCEDURE sp_GetUserContact
    @Id NVARCHAR(100)
AS
BEGIN
    SELECT Email, PhoneNumber 
    FROM vw_GetAllUsers
    WHERE Id = @Id;
END