CREATE OR ALTER PROCEDURE sp_GetNotification
    @UserId NVARCHAR(101),
    @Id INT
AS 
BEGIN 
    SELECT * FROM vw_GetAllNotifications
    WHERE UserId = @UserId AND Id = @Id;
END