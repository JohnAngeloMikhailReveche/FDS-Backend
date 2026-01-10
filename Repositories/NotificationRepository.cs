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

    /// <summary>
    /// Get all notifications for a specific user ordered by creation date (newest first)
    /// </summary>
    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(string targetUserId)
    {
        const string sql = """
            SELECT * FROM Notifications
            WHERE TargetUserId = @targetUserId
            ORDER BY CreatedAt DESC; 
        """;
        
        using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<Notification>(sql, new { targetUserId });
    }

    /// <summary>
    /// Get a specific notification by ID for a user
    /// </summary>
    public async Task<Notification?> GetNotificationAsync(string targetUserId, int id)
    {
        const string sql = """
            SELECT * FROM Notifications
            WHERE TargetUserId = @targetUserId AND Id = @id;
        """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleOrDefaultAsync<Notification>(
            sql,
            new { targetUserId, id }
        );
    }

    /// <summary>
    /// Add a new notification to the database
    /// </summary>
    public async Task<int> AddNotificationAsync(Notification notification)
    {
        const string sql = """
            INSERT INTO Notifications 
            (TargetUserId, Type, Title, Message, Status, PhoneNumber, EmailAddress, IsRead, CreatedAt, UpdatedAt)
            VALUES 
            (@TargetUserId, @Type, @Title, @Message, @Status, @PhoneNumber, @EmailAddress, @IsRead, @CreatedAt, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);
        """;

        using var conn = new SqlConnection(_connectionString);
        var id = await conn.ExecuteScalarAsync<int>(sql, notification);
        return id;
    }

    /// <summary>
    /// Update an existing notification
    /// </summary>
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
                IsRead = @IsRead,
                ReadAt = @ReadAt,
                UpdatedAt = @UpdatedAt,
            WHERE Id = @Id AND TargetUserId = @TargetUserId;
        """;

        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, notification);
    }

    /// <summary>
    /// Mark all unread notifications as read for a user
    /// </summary>
    public async Task<bool> MarkAllAsReadAsync(string targetUserId)
    {
        const string sql = """
            UPDATE Notifications
            SET IsRead = 1, ReadAt = @readAt
            WHERE TargetUserId = @targetUserId AND IsRead = 0;
        """;

        using var conn = new SqlConnection(_connectionString);
        var rowsAffected = await conn.ExecuteAsync(sql, new { targetUserId, readAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    public async Task<bool> MarkAsReadAsync(string targetUserId, int id)
    {
        const string sql = """
            UPDATE Notifications
            SET IsRead = 1, ReadAt = @readAt, UpdatedAt = @updatedAt
            WHERE Id = @id AND TargetUserId = @targetUserId;
        """;

        using var conn = new SqlConnection(_connectionString);
        var rowsAffected = await conn.ExecuteAsync(
            sql, 
            new { id, targetUserId, readAt = DateTime.UtcNow, updatedAt = DateTime.UtcNow }
        );
        return rowsAffected > 0;
    }

    /// <summary>
    /// Delete all notifications for a user
    /// </summary>
    public async Task<bool> DeleteAllNotificationsAsync(string targetUserId)
    {
        const string sql = """
            DELETE FROM Notifications
            WHERE TargetUserId = @targetUserId;
        """;

        using var conn = new SqlConnection(_connectionString);
        var rowsAffected = await conn.ExecuteAsync(sql, new { targetUserId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Delete a specific notification
    /// </summary>
    public async Task<bool> DeleteNotificationAsync(string targetUserId, int id)
    {
        const string sql = """
            DELETE FROM Notifications 
            WHERE TargetUserId = @targetUserId AND Id = @id;
        """;

        using var conn = new SqlConnection(_connectionString);
        var rowsAffected = await conn.ExecuteAsync(sql, new { targetUserId, id });
        return rowsAffected > 0;
    }
}