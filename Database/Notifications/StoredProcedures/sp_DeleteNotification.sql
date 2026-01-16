CREATE OR ALTER PROCEDURE sp_DeleteNotification
    @UserId NVARCHAR(100),
    @Id INT
AS
BEGIN 
    DELETE FROM Notifications
    WHERE UserId = @UserId AND Id = @Id;
END