CREATE OR ALTER PROCEDURE sp_DeleteAllNotifications
    @UserId NVARCHAR(100)
AS
BEGIN
    DELETE FROM Notifications
    WHERE UserId = @UserId;
END