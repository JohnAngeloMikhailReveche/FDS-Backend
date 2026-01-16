CREATE OR ALTER PROCEDURE sp_GetAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    SELECT * FROM vw_GetAllNotifications
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END