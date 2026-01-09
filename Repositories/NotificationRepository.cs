using Dapper;
using Microsoft.Data.SqlClient;
using NotificationService.Models;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString; 
    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NotificationDatabase") 
            ?? throw new InvalidOperationException("Connection string 'NotificationDatabase' is not configured.");
    }

   public async Task<IEnumerable<Notification?>> GetAllNotificationsAsync(int userId)
   {
        const string sql = """
            SELECT * FROM Notifications
            WHERE userId = @userId; 
        """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<Notification>(
            sql, 
            new {userId}
        );
   } 

   public async Task<Notification?> GetNotificationAsync(int userId, int notifId)
   {
        const string sql = """
            SELECT * FROM Notifications
            WHERE userId = @userId AND notifId = @notifId;
        """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<Notification>(
            sql,
            new {userId, notifId}
        );
        
    }

    public async Task<int> AddNotificationAsync(Notification notification)
    {
        const string sql = """
            INSERT INTO Notifications (Id, UserId, Type, Title, Message, Status, PhoneNumber, EmailAddress, CreatedAt, UpdatedAt)
            VALUES (@Id, @UserId, @Type, @Title, @Message, @Status, @PhoneNumber, @EmailAddress, @CreatedAt, @UpdatedAt); 
        """;

        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, notification);
        return notification.Id; 
    }

    public async Task UpdateNotificationAsync(Notification notification)
    {
        const string sql = """
            UPDATE Notifications
            SET Type = @Type, 
                Title = @Title, 
                Message = @Message, 
                Status = @Status, 
                PhoneNumber = @PhoneNumber, 
                EmailAddress = @EmailAddress, 
                CreatedAt = @CreatedAt, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND UserId = @UserId
        """;

        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, notification);
    }

    public async Task DeleteAllNotificationsAsync(int userId)
    {
        const string sql = """
              DELETE FROM Notifications
              WHERE userId = @userId;
        """;

       using var conn = new SqlConnection(_connectionString);
         await conn.ExecuteAsync(sql, new { userId });
    }

    public async Task DeleteNotificationAsync(int userId, int notifId)
    {
        const string sql = """
            DELETE FROM Notifications 
            WHERE userId = @userId AND notifId = @notifId;
        """;

        using var conn = new SqlConnection(_connectionString);
           await conn.ExecuteAsync(sql, new { userId, notifId });
    }
}